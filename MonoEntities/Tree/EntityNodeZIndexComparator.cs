using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonoEntities.Tree
{
    [ExcludeFromCodeCoverage]
    public class EntityNodeZIndexComparator : IComparer<EntityNode>
    {
        public int Compare(EntityNode x, EntityNode y)
        {
            if (x != null && y != null)
                return x.Entity.Transform.ZIndex.CompareTo(y.Entity.Transform.ZIndex);

            return 0;
        }
    }
}
