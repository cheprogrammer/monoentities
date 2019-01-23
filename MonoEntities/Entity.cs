using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEntities
{
    public class Entity
    {
        public int Id { get; internal set; }

        public string Name { get; set; }

        public object Tag { get; set; } = null;


        internal bool Started { get; set; } = false;

        public bool Enabled { get; set; } = true;

        public bool EnabledInHierarchy => (Transform.Parent?.Entity.EnabledInHierarchy ?? Enabled) && Enabled;

        internal bool MarkedToBeRemoved { get; set; } = false;

        internal bool Processable => Started && Enabled && EnabledInHierarchy && !MarkedToBeRemoved;


        internal ComponentCollection Components { get; } = new ComponentCollection();

        internal EcsService Service { get; }
        
        public Transform2DComponent Transform { get; internal set; }


        internal Entity(EcsService service)
        {
            Service = service;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return (T)AddComponent(typeof(T));
        }

        public Component AddComponent(Type componentType)
        {
            return Service.AddComponent(this, componentType);
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)GetComponent(typeof(T));
        }

        public Component GetComponent(Type componentType)
        {
            return Service.GetComponent(this, componentType);
        }

        public IEnumerable<Component> GetComponents()
        {
            return Service.GetComponents(this);
        }

        public void RemoveComponent<T>() where T : Component, new()
        {
            RemoveComponent(typeof(T));
        }

        public void RemoveComponent(Type componentType)
        {
            if(componentType == typeof(Transform2DComponent))
                throw new ArgumentException($"Cannot remove component {typeof(Transform2DComponent).Name}: this component is obligatory");

            Service.RemoveComponent(this, componentType);
        }

        public void Destroy()
        {
            Service.Destroy(this);
        }

        #region Components Searching

        public T FindComponent<T>() where T: Component
        {
            return (T)FindComponent(typeof(T));
        }

        public Component FindComponent(Type componentType)
        {
            return Service.FindComponent(componentType);
        }

        public IEnumerable<T> FindComponents<T>() where T : Component
        {
            return FindComponents(typeof(T)).OfType<T>();
        }

        public IEnumerable<Component> FindComponents(Type componentType)
        {
            return Service.FindComponents(componentType);
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return (T)GetComponentInParent(typeof(T));
        }

        public Component GetComponentInParent(Type componentType)
        {
            return Service.GetComponentInParent(this, componentType);
        }

        public IEnumerable<T> GetComponentsInParent<T>()
        {
            return GetComponentsInParent(typeof(T)).OfType<T>();
        }

        public IEnumerable<Component> GetComponentsInParent(Type componentType)
        {
            return Service.GetComponentsInParent(this, componentType);
        }

        public T GetComponentInChild<T>() where T : Component
        {
            return (T)GetComponentInChild(typeof(T));
        }

        public Component GetComponentInChild(Type componentType)
        {
            return Service.GetComponentInChild(this, componentType);
        }

        public IEnumerable<T> GetComponentsInChild<T>() where T: Component
        {
            return GetComponentsInChild(typeof(T)).OfType<T>();
        }

        public IEnumerable<Component> GetComponentsInChild(Type componentType)
        {
            return Service.GetComponentsInChild(this, componentType);
        }

        #endregion

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"\"{Name ?? "Entity"}\" ({Id})";
        }
    }
}
