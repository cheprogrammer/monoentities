using System.Collections.Generic;

namespace MonoEntities.Tree
{
    internal class EntityNodeComparator : IComparer<EntityNode>
    {
        public int Compare(EntityNode x, EntityNode y)
        {
            if (x != null && y != null)
                return x.Entity.Transform.ZIndex.CompareTo(y.Entity.Transform.ZIndex);

            return 0;
        }
    }
}
