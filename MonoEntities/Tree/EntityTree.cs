using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        internal void AddEntity(Entity entity)
        {
            if(_entityNodes.ContainsKey(entity))
                throw new Exception($"Cannot add entity \"{entity}\": entity was already added");

            var parentNode = FindParent(entity);

            if (parentNode == null)
                throw new Exception($"Cannot add entity \"{entity}\": entity parent not exists in tree");

            var node = parentNode.AddChild(entity);
            _entityNodes.Add(entity, node);
        }

        internal void RemoveEntity(Entity entity)
        {
            if (!_entityNodes.ContainsKey(entity))
                throw new Exception($"Cannot remove entity \"{entity}\": entity was not added");

            var parentNode = FindParent(entity);

            if (parentNode == null)
                throw new Exception($"Cannot remove entity \"{entity}\": entity parent not exists in tree");

            parentNode.RemoveChild(entity);
            _entityNodes.Remove(entity);
        }

        internal void ChangeParent(Entity entity, Entity oldParent, Entity newParent)
        {
            var parentNode = FindNode(oldParent);
            var newParentNode = FindNode(newParent);

            var node = parentNode.DetachChild(entity);
            newParentNode.AttachChild(node);
        }

        internal void UpdateSiblingsZIndex(Entity entity)
        {
            FindParent(entity).UpdateZIndex(FindNode(entity));
        }
    }
}
