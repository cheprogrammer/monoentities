using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoEntities
{
    public abstract class Component
    {
        public Entity Entity { get; internal set; }

        internal EcsService Service => Entity.Service;

        internal bool Started { get; set; } = false;

        public Transform2DComponent Transform => Entity.Transform;

        internal bool MarkedToBeRemoved { get; set; } = false;

        internal bool Processable => Started && !MarkedToBeRemoved && Entity.EnabledInHierarchy;

        [ExcludeFromCodeCoverage]
        protected internal Component()
        {

        }

        protected internal virtual void OnEnable()
        {

        }

        protected internal virtual void Start()
        {

        }

        protected internal virtual void Update(GameTime gametime)
        {

        }

        protected internal virtual void BeforeDraw(GameTime gametime)
        {

        }

        protected internal virtual void Draw(GameTime gameTime)
        {

        }

        protected internal virtual void AfterDraw(GameTime gametime)
        {

        }

        protected internal virtual void OnDisable()
        {

        }

        protected internal virtual void OnDestroy()
        {

        }

        public virtual void Reset()
        {

        }

        public void Destroy()
        {
            Entity.RemoveComponent(GetType());
        }

        #region Component Operations

        protected internal Entity CreateEntityFromTemplate<T>(params object[] args) where T : EntityTemplate
        {
            return Service.CreateEntityFromTemplate<T>(args);
        }

        protected internal Entity CreateEntityFromTemplate(string templateName, params object[] args)
        {
            return Service.CreateEntityFromTemplate(templateName, args);
        }

        protected internal Entity CreateEntity()
        {
            return Service.CreateEntity();
        }

        protected internal T AddComponent<T>() where T : Component, new()
        {
            return (T)Service.AddComponent(Entity, typeof(T));
        }

        protected internal Component AddComponent(Type componentType)
        {
            return Service.AddComponent(Entity, componentType);
        }

        protected internal T GetComponent<T>() where T : Component
        {
            return (T)Service.GetComponent(Entity, typeof(T));
        }

        protected internal Component GetComponent(Type componentType)
        {
            return Service.GetComponent(Entity, componentType);
        }

        protected internal void RemoveComponent<T>() where T : Component, new()
        {
            RemoveComponent(typeof(T));
        }

        protected internal void RemoveComponent(Type componentType)
        {
            Service.RemoveComponent(Entity, componentType);
        }

        protected internal T FindComponent<T>() where T : Component
        {
            return FindComponents<T>().FirstOrDefault();
        }

        protected internal Component FindComponent(Type componentType)
        {
            return FindComponents(componentType).FirstOrDefault();
        }

        protected internal IEnumerable<T> FindComponents<T>() where T : Component
        {
            return FindComponents(typeof(T)).OfType<T>();
        }

        protected internal IEnumerable<Component> FindComponents(Type componentType)
        {
            return Service.FindComponents(componentType);
        }

        protected internal T GetComponentInParent<T>() where T : Component
        {
            return (T)GetComponentInParent(typeof(T));
        }

        protected internal Component GetComponentInParent(Type componentType)
        {
            return Service.GetComponentInParent(Entity, componentType);
        }

        protected internal IEnumerable<T> GetComponentsInParent<T>()
        {
            return GetComponentsInParent(typeof(T)).OfType<T>();
        }

        protected internal IEnumerable<Component> GetComponentsInParent(Type componentType)
        {
            return Service.GetComponentsInParent(Entity, componentType);
        }

        protected internal T GetComponentInChild<T>() where T : Component
        {
            return (T)GetComponentInChild(typeof(T));
        }

        protected internal Component GetComponentInChild(Type componentType)
        {
            return Service.GetComponentInChild(Entity, componentType);
        }

        protected internal IEnumerable<T> GetComponentsInChild<T>() where T : Component
        {
            return GetComponentsInChild(typeof(T)).OfType<T>();
        }

        protected internal IEnumerable<Component> GetComponentsInChild(Type componentType)
        {
            return Service.GetComponentsInChild(Entity, componentType);
        }

        #endregion

    }
}
