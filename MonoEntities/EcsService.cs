using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities.Tree;
using NLog;

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

        private readonly List<Entity> _entities = new List<Entity>(128);

        private readonly Dictionary<Type, List<Component>> _componentsCache = new Dictionary<Type, List<Component>>();

        internal bool IsDrawing { get; set; } = false;


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

            BeforeUpdate?.Invoke();

            foreach (Entity entity in _entities)
            {
                if (!entity.Processable)
                    continue;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < entity.Components.Count; i++)
                {
                    var entityComponent = entity.Components[i];

                    if (entityComponent.Processable)
                        entityComponent.Update(gameTime);
                }
            }

            AfterUpdate?.Invoke();

            ProcessEntitiesForRemoving();
            ProcessComponentsForRemoving();
        }

        /// <summary>
        /// Performs the drawning of current entities and components
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            IsDrawing = true;

            BeforeDraw?.Invoke();

            foreach (EntityNode entityNode in Tree.ChildNodes)
            {
                DrawNode(entityNode, gameTime);
            }

            AfterDraw?.Invoke();

            IsDrawing = false;
        }

        private void DrawNode(EntityNode node, GameTime gameTime)
        {
            var entity = node.Entity;

            if (!entity.Processable)
                return;

            // Draw child first
            foreach (EntityNode childNode in node.ChildNodes)
            {
                DrawNode(childNode, gameTime);
            }

            if (!entity.Processable)
                return;

            // After arr children, draw current entity
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entity.Components.Count; i++)
            {
                var entityComponent = entity.Components[i];

                if (entityComponent.Processable)
                {
                    entityComponent.BeforeDraw(gameTime);
                    entityComponent.Draw(gameTime);
                    entityComponent.AfterDraw(gameTime);
                }
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
        public Entity CreateEntityFromTemplate<T>(params object[] args) where T : EntityTemplate
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
                result = CreateEntityFromTemplatePrivate(template, args);
            }
            else
            {
                throw new ArgumentException($"Unable to build entity from template '{templateType.Name}': this template was not registered", nameof(templateType));
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
                result = CreateEntityFromTemplatePrivate(template, args);
            }
            else
            {
                throw new ArgumentException($"Unable to build entity from template '{templateName}': this template was not registered", nameof(templateName));
            }

            return result;
        }

        private Entity CreateEntityFromTemplatePrivate(EntityTemplate template, params object[] args)
        {
            Entity result = CreateEntity();
            template.BuildEntity(result, args);

            return result;
        }

        /// <summary>
        /// Creates empty entity with default transform Component
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            if (IsDrawing)
                throw new EcsWorkflowException("Cannot create entity during Draw phase");

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

            Tree.AddEntity(entity);

            _entitiesForAdding.Enqueue(entity);

            return entity;
        }

        internal void Destroy(Entity entity)
        {
            if (entity.MarkedToBeRemoved)
                throw new EcsWorkflowException("Cannot destroy already destroyed entity");

            foreach (EntityNode entityNode in Tree.FindNode(entity).Reverse())
            {
                DestroyEntity(entityNode.Entity);
            }
        }

        private void DestroyEntity(Entity entity)
        {
            if (IsDrawing)
                throw new EcsWorkflowException("Cannot destroy entity during Draw phase");

            entity.Enabled = false;
            entity.MarkedToBeRemoved = true;

            for (int i = entity.Components.Count - 1; i >= 0; i--)
            {
                Component component = entity.Components[i];
                RemoveComponent(entity, component.GetType());
            }

            Tree.RemoveEntity(entity);

            _entitiesForRemoving.Enqueue(entity);
        }

        #endregion

        #region Entity Component Manipulation

        internal Component AddComponent(Entity entity, Type componentType)
        {
            if (IsDrawing)
                throw new EcsWorkflowException("Cannot add component during Draw phase");

            Component component = (Component)Activator.CreateInstance(componentType);
            component.Entity = entity;

            entity.Components.Add(component);

            AddComponentToCache(componentType, component);

            _componentsForAdding.Enqueue(component);

            return component;
        }

        internal void RemoveComponent(Entity entity, Type componentType)
        {
            if (IsDrawing)
                throw new EcsWorkflowException("Cannot remove component during Draw phase");

            Component component = entity.Components[componentType];

            if(component.MarkedToBeRemoved)
                throw new EcsWorkflowException("Cannot remove already destroyed component");

            component.MarkedToBeRemoved = true;

            _componentsForRemoving.Enqueue(component);
        }

        internal Component GetComponent(Entity entity, Type componentType)
        {
            if (entity.Components.TryGetValue(componentType, out var result))
            {
                if(!result.MarkedToBeRemoved)
                    return result;
            }

            return null;
        }

        #endregion

        #region Components Searching

        public T FindComponent<T>() where T: Component
        {
            return (T)FindComponents(typeof(T)).FirstOrDefault();
        }

        public IEnumerable<T> FindComponents<T>(Type componentType) where T: Component
        {
            return FindComponents(typeof(T)).OfType<T>();
        }

        public Component FindComponent(Type componentType)
        {
            return FindComponents(componentType).FirstOrDefault();
        }

        public IEnumerable<Component> FindComponents(Type componentType)
        {
            if (_componentsCache.TryGetValue(componentType, out var result))
            {
                return result.Where(e => !e.MarkedToBeRemoved).ToList();
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

                _entities.Add(entity);
                entity.Started = true;
            }
        }

        private void ProcessEntitiesForRemoving()
        {
            while (_entitiesForRemoving.Count != 0)
            {
                Entity entity = _entitiesForRemoving.Dequeue();

                _entities.Remove(entity);
                _freeIds.Enqueue(entity.Id);
            }
        }

        private void ProcessComponentForAdding()
        {
            while (_componentsForAdding.Count != 0)
            {
                Component component = _componentsForAdding.Dequeue();

                if (component.MarkedToBeRemoved)
                    continue;

                component.Start();
                component.Started = true;

                component.OnEnable();
            }
        }

        private void ProcessComponentsForRemoving()
        {
            while (_componentsForRemoving.Count != 0)
            {
                Component component = _componentsForRemoving.Dequeue();
                Entity entity = component.Entity;

                component.OnDisable();
                component.OnDestroy();

                RemoveComponentFromCache(component.GetType(), component);
                entity.Components.Remove(component);
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

        #region Events

        public event Action BeforeUpdate;

        public event Action AfterUpdate;

        public event Action BeforeDraw;

        public event Action AfterDraw;
        #endregion
    }
}

