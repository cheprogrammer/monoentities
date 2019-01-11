using System;
using System.Collections.Generic;

namespace MonoEntities.Tree
{
    public class EntityTree : EntityNode
    {
        private readonly Dictionary<Entity, EntityNode> _entityNodes = new Dictionary<Entity, EntityNode>();

        internal EntityNode FindNode(Entity entity)
        {
            if (entity == null)
                return this;

            if (_entityNodes.TryGetValue(entity, out var node))
            {
                return node;
            }

            return null;
        }

        internal EntityNode FindParent(Entity entity)
        {
            if (entity.Transform.Parent == null)
            {
                return this;
            }
            else
            {
                return FindNode(entity.Transform.Parent.Entity);
            }
        }

        /// <summary>
        /// Adds new Entity to the tree
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal bool AddEntity(Entity entity)
        {
            var parentNode = FindParent(entity);

            if (parentNode == null)
                return false;

            var node = parentNode.AddChild(entity);
            _entityNodes.Add(entity, node);

            return true;
        }

        /// <summary>
        /// Removes Entity with empty child nodes from tree
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal bool RemoveEntity(Entity entity)
        {
            var parentNode = FindParent(entity);

            if (parentNode == null)
                return false;

            parentNode.RemoveChild(entity);
            _entityNodes.Remove(entity);

            return true;
        }

        /// <summary>
        /// Updates the parent of entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        internal void ChangeParent(Entity entity, Entity oldParent, Entity newParent)
        {
            var parentNode = FindNode(oldParent);
            var newParentNode = FindNode(newParent);

            var node = parentNode.DetachChild(entity);
            newParentNode.AttachChild(node);
        }

        /// <summary>
        /// Updates the parent child nodes order
        /// </summary>
        /// <param name="entity"></param>
        internal void UpdateSiblingsZIndex(Entity entity)
        {
            FindParent(entity).SortChildren();
        }
    }
}
