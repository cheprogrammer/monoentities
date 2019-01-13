using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonoEntities.Tree
{
    [ExcludeFromCodeCoverage]
    public class EntityNode : IEnumerable<EntityNode>
    {
        internal static IComparer<EntityNode> Comparator = new EntityNodeComparator();

        public Entity Entity { get; }

        public bool IsRoot => Entity == null;

        protected EntityNode Parent { get; set; }

        internal List<EntityNode> ChildNodes { get; } = new List<EntityNode>();

        internal EntityNode(Entity entity = null, EntityNode parent = null)
        {
            Entity = entity;
            Parent = parent;
        }

        public IEnumerator<EntityNode> GetEnumerator()
        {
            if(!IsRoot)
                yield return this;

            foreach (EntityNode entityNode in ChildNodes)
            {
                foreach (EntityNode childNode in entityNode)
                {
                    yield return childNode;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal EntityNode AddChild(Entity entity)
        {
            EntityNode node = new EntityNode(entity, this);
            ChildNodes.Add(node);

            SortChildren();

            return node;
        }

        internal void RemoveChild(Entity entity)
        {
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                var node = ChildNodes[i];

                if (node.Entity == entity)
                {
                    if(node.ChildNodes.Count != 0)
                        throw new Exception("Cannot remove non-empty child node with children. Destroy children first!");

                    ChildNodes.RemoveAt(i);
                    return;
                }
            }

            throw new Exception("Cannot remove child node: child node not found");
        }

        internal void AttachChild(EntityNode node)
        {
            ChildNodes.Add(node);
            node.Parent = this;
            SortChildren();
        }

        internal EntityNode DetachChild(Entity entity)
        {
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                var node = ChildNodes[i];

                if (node.Entity == entity)
                {
                    ChildNodes.RemoveAt(i);
                    node.Parent = null;
                    return node;
                }
            }

            throw new Exception("Cannot detach child node: child node not found");
        }

        internal void SortChildren()
        {
            ChildNodes.Sort(Comparator);
        }
    }
}

