using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities.Components;
using MonoEntities.Tree;
using NLog;
using NLog.Fluent;

namespace MonoEntities
{
    public class EcsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EntityTree Tree { get; } = new EntityTree();

        private int _maxId = 0;

        private readonly Queue<int> _freeIds = new Queue<int>(128);

        private readonly Dictionary<Type, EntityTemplate> _templates = new Dictionary<Type, EntityTemplate>();

        private readonly Dictionary<string, EntityTemplate> _templatesByName = new Dictionary<string, EntityTemplate>();

        private readonly Queue<Entity> _entitiesForAdding = new Queue<Entity>();

        private readonly Queue<Entity> _entitiesForRemoving = new Queue<Entity>();

        private readonly Queue<Component> _componentsForAdding = new Queue<Component>();

        private readonly Queue<Component> _componentsForRemoving = new Queue<Component>();

        private readonly Queue<TransformPropertyChangedRequest> _transformChangeRequests = new Queue<TransformPropertyChangedRequest>();

        private readonly Dictionary<Type, List<Component>> _componentsCache = new Dictionary<Type, List<Component>>();

        internal EcsService(IEnumerable<Type> availableTemplates)
        {
            // processing templates
            foreach (Type availableTemplateType in availableTemplates)
            {
                EntityTemplate template = null;
                try
                {
                    template = (EntityTemplate)Activator.CreateInstance(availableTemplateType);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Unable to create template '{availableTemplateType.Name}'");
                    continue;
                }

                template.Initialize();

                _templates[availableTemplateType] = template;
                _templatesByName[template.Name] = template;
            }
        }

        #region Draw and Update logic

        /// <summary>
        /// Performs update step for all entities and components
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            ProcessEntitiesForAdding();
            ProcessComponentForAdding();

            foreach (EntityNode entityNode in Tree.ChildNodes)
            {
                UpdateNode(entityNode, gameTime);
            }

            ProcessTransformChanges();

            ProcessEntitiesForRemoving();
            ProcessComponentsForRemoving();
        }

        private void UpdateNode(EntityNode node, GameTime gameTime)
        {
            var entity = node.Entity;

            if (!entity.Processable)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < entity.Components.Count; i++)
            {
                Component entityComponent = entity.Components[i];

                if (entityComponent.Processable)
                    entityComponent.Update(gameTime);
            }

            foreach (EntityNode childNode in node.ChildNodes)
            {
                UpdateNode(childNode, gameTime);
            }
        }

        /// <summary>
        /// Performs the drawning of current entities and components
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            foreach (EntityNode entityNode in Tree.ChildNodes)
            {
                DrawNode(entityNode, gameTime);
            }
        }

        private void DrawNode(EntityNode node, GameTime gameTime)
        {
            var entity = node.Entity;

            if (!entity.Processable)
                return;

            foreach (EntityNode childNode in node.ChildNodes)
            {
                DrawNode(childNode, gameTime);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < entity.Components.Count; i++)
            {
                Component entityComponent = entity.Components[i];
                entityComponent.Draw(gameTime);
            }
        }

        #endregion

        #region Entity creation and deletion

        /// <summary>
        /// Creates entity from template '<typeparamref name="T"/>'
        /// </summary>
        /// <typeparam name="T">Template Type</typeparam>
        /// <param name="args">Argumenst which will be passed to template</param>
        /// <returns></returns>
        public Entity CreateEntityFromTemplate<T>(params object[] args) where T : EntityTemplate, new()
        {
            return CreateEntityFromTemplate(typeof(T), args);
        }

        /// <summary>
        /// Creates entity from '<paramref name="templateType"/>' template
        /// </summary>
        /// <param name="templateType">Type of template</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Entity CreateEntityFromTemplate(Type templateType, params object[] args)
        {
            Entity result = null;

            if (_templates.TryGetValue(templateType, out var template))
            {
                result = CreateEntityFromTemplateInternal(template, args);
            }
            else
            {
                Log.Error($"Unable to build entity from template '{templateType.Name}': this template does not registered");
            }

            return result;
        }

        /// <summary>
        /// Creates entuty from template by its name
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Entity CreateEntityFromTemplate(string templateName, params object[] args)
        {
            Entity result = null;

            if (_templatesByName.TryGetValue(templateName, out var template))
            {
                result = CreateEntityFromTemplateInternal(template, args);
            }
            else
            {
                Log.Error($"Unable to build entity from template with name '{templateName}': this template does not registered in system");
            }

            return result;
        }

        internal Entity CreateEntityFromTemplateInternal(EntityTemplate template, params object[] args)
        {
            Entity result = CreateEntity();
            template.BuildEntity(result, args);

            return result;
        }

        internal Entity CreateEntity()
        {
            Entity entity = new Entity(this);

            if (_freeIds.Count != 0)
            {
                entity.Id = _freeIds.Dequeue();
            }
            else
            {
                entity.Id = ++_maxId;
            }

            Transform2DComponent transform = (Transform2DComponent)AddComponent(entity, typeof(Transform2DComponent));
            entity.Transform = transform;

            _entitiesForAdding.Enqueue(entity);

            return entity;
        }

        internal void DestroyEntityAndChildren(Entity entity)
        {
            if (entity.IsInHierarchy)
            {
                EntityNode node = Tree.FindNode(entity);
                DestroyNode(node);
            }
            else
            {
                DestroyEntity(entity);
            }
        }

        private void DestroyNode(EntityNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                DestroyNode(childNode);
            }

            DestroyEntity(node.Entity);
        }

        private void DestroyEntity(Entity entity)
        {
            entity.MarkedToBeRemoved = true;

            for (int i = entity.Components.Count - 1; i >= 0; i--)
            {
                Component component = entity.Components[i];
                RemoveComponent(entity, component.GetType());
            }

            _entitiesForRemoving.Enqueue(entity);
        }

        #endregion

        #region Entity Component Manipulation

        internal Component AddComponent(Entity entity, Type componentType)
        {
            Component component = (Component)Activator.CreateInstance(componentType);
            component.Entity = entity;

            entity.Components.Add(component);

            AddComponentToCache(componentType, component);

            _componentsForAdding.Enqueue(component);

            return component;
        }

        internal void RemoveComponent(Entity entity, Type componentType)
        {
            Component component = entity.Components[componentType];
            component.MarkedToBeRemoved = true;

            _componentsForRemoving.Enqueue(component);
        }

        internal Component GetComponent(Entity entity, Type componentType)
        {
            if (entity.Components.TryGetValue(componentType, out var result))
            {
                return result;
            }

            return null;
        }

        #endregion

        #region Components Searching

        internal Component FindComponent(Type componentType)
        {
            return FindComponents(componentType).FirstOrDefault();
        }

        internal IEnumerable<Component> FindComponents(Type componentType)
        {
            if (_componentsCache.TryGetValue(componentType, out var result))
            {
                return result;
            }

            return new Component[0];
        }

        internal Component GetComponentInParent(Entity entity, Type componentType)
        {
            Transform2DComponent transform = entity.Transform;

            while (transform != null)
            {
                var component = GetComponent(transform.Entity, componentType);

                if (component != null)
                {
                    return component;
                }

                transform = transform.Parent;
            }

            return null;
        }

        internal IEnumerable<Component> GetComponentsInParent(Entity entity, Type componentType)
        {
            Transform2DComponent transform = entity.Transform;
            List<Component> result = new List<Component>();

            while (transform != null)
            {
                var component = GetComponent(transform.Entity, componentType);

                if (component != null)
                {
                    result.Add(component);
                }

                transform = transform.Parent;
            }

            return result;
        }

        internal Component GetComponentInChild(Entity entity, Type componentType)
        {
            if(!entity.IsInHierarchy)
                throw new Exception($"Cannot find component of type \"{componentType.Name}\" in child: Entity \"{entity}\" is not in hierarchy.");

            EntityNode node = Tree.FindNode(entity);

            foreach (EntityNode entityNode in node)
            {
                Component component = GetComponent(entityNode.Entity, componentType);

                if (component != null)
                    return component;
            }

            return null;
        }

        internal IEnumerable<Component> GetComponentsInChild(Entity entity, Type componentType)
        {
            if (!entity.IsInHierarchy)
                throw new Exception($"Cannot find component of type \"{componentType.Name}\" in child: Entity \"{entity}\" is not in hierarchy.");

            EntityNode node = Tree.FindNode(entity);
            List<Component> result = new List<Component>();

            foreach (EntityNode entityNode in node)
            {
                Component component = GetComponent(entityNode.Entity, componentType);

                if (component != null)
                    result.Add(component);
            }

            return result;
        }

        #endregion

        #region Processing Entities and Components

        private void ProcessEntitiesForAdding()
        {
            while (_entitiesForAdding.Count != 0)
            {
                Entity entity = _entitiesForAdding.Dequeue();

                if (entity.MarkedToBeRemoved)
                    continue;

                if (Tree.AddEntity(entity))
                {
                    entity.Started = true;
                    entity.IsInHierarchy = true;

                    entity.Transform.PropertyChanged += TransformOnPropertyChanged;
                }
                else
                {
                    // if entity was not added to tree - that means that parent node was destroyed before it was registered in hierarchy
                    // necessary to destory this entity as entity
                    DestroyEntity(entity);
                }
            }
        }

        private void ProcessComponentForAdding()
        {
            while (_componentsForAdding.Count != 0)
            {
                Component component = _componentsForAdding.Dequeue();

                if (component.MarkedToBeRemoved)
                    continue;

                component.Started = true;
                component.Start();

                if (component.Enabled)
                    component.OnEnable();
                else
                    component.OnDisable();
            }
        }

        private void ProcessEntitiesForRemoving()
        {
            while (_entitiesForRemoving.Count != 0)
            {
                Entity entity = _entitiesForRemoving.Dequeue();

                if (entity.Started)
                {
                    if (Tree.RemoveEntity(entity))
                    {
                        entity.Transform.PropertyChanged -= TransformOnPropertyChanged;
                    }

                    entity.IsInHierarchy = false;
                }
            }
        }

        private void ProcessComponentsForRemoving()
        {
            while (_componentsForRemoving.Count != 0)
            {
                Component component = _componentsForRemoving.Dequeue();
                Entity entity = component.Entity;
                component.Enabled = false;
                component.OnDestroy();

                RemoveComponentFromCache(component.GetType(), component);
                entity.Components.Remove(component);
            }
        }

        private void ProcessTransformChanges()
        {
            while (_transformChangeRequests.Count != 0)
            {
                TransformPropertyChangedRequest request = _transformChangeRequests.Dequeue();

                string propertyName = request.Arguments.PropertyName;

                if (propertyName == nameof(Transform2DComponent.ZIndex))
                {
                    Tree.UpdateSiblingsZIndex(request.Sender.Entity);
                }
                else if (propertyName == nameof(Transform2DComponent.Parent))
                {
                    var oldParent = (Transform2DComponent)request.Arguments.OldValue;
                    var newParent = (Transform2DComponent)request.Arguments.NewValue;

                    Tree.ChangeParent(request.Sender.Entity, oldParent?.Entity, newParent?.Entity);
                }
            }
        }

        #endregion

        #region Components Caching

        private void AddComponentToCache(Type componentType, Component component)
        {
            if (_componentsCache.TryGetValue(componentType, out var list))
            {
                list.Add(component);
            }
            else
            {
                _componentsCache.Add(componentType, new List<Component>(128)
                {
                    component
                });
            }
        }

        private void RemoveComponentFromCache(Type componentType, Component component)
        {
            if (_componentsCache.TryGetValue(componentType, out var list))
            {
                list.Remove(component);
            }
        }

        #endregion

        private void TransformOnPropertyChanged(object sender, PropertyChangedExtendedEventArgs propertyChangedExtendedEventArgs)
        {
            _transformChangeRequests.Enqueue(new TransformPropertyChangedRequest((Transform2DComponent)sender, propertyChangedExtendedEventArgs));
        }
    }
}

