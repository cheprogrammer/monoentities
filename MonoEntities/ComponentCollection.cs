using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEntities
{
    internal class ComponentCollection : IEnumerable<Component>
    {
        private IList<Component> Components { get; } = new List<Component>(16);

        private IDictionary<Type, Component> ComponentsByType { get; } = new Dictionary<Type, Component>();

        public IEnumerator<Component> GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Component item)
        {
            Type componentType = item.GetType();

            if (ComponentsByType.ContainsKey(componentType))
            {
                throw new ArgumentException($"Cannot add component of type \"{componentType.Name}\": component of same type already added.");
            }

            Components.Add(item);
            ComponentsByType.Add(componentType, item);
        }

        public void Clear()
        {
            Components.Clear();
            ComponentsByType.Clear();
        }

        public bool Contains(Component item)
        {
            return ComponentsByType.ContainsKey(item.GetType());
        }

        public bool ContainsKey(Type componentType)
        {
            return ComponentsByType.ContainsKey(componentType);
        }

        public bool TryGetValue(Type componenType, out Component value)
        {
            return ComponentsByType.TryGetValue(componenType, out value);
        }

        public bool Remove(Component item)
        {
            Type componentType = item.GetType();
            return Components.Remove(item) && ComponentsByType.Remove(componentType);
        }

        public bool Remove(Type componentType)
        {
            if (!TryGetValue(componentType, out var value))
            {
                throw new ArgumentException($"Cannot remove component of type \"{componentType.Name}\": component are not attached.");
            }

            return Components.Remove(value) && ComponentsByType.Remove(componentType);
        }

        public Component this[int index] => Components[index];

        public Component this[Type index]
        {
            get
            {
                ComponentsByType.TryGetValue(index, out var result);
                return result;
            }
            
        }

        public int Count => Components.Count;
    }
}
