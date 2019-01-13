using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEntities.Tree;

namespace MonoEntities.Extensions
{
    public static class ListExtension
    {
        public static void InsertElementAscending(this List<EntityNode> source, EntityNode element)
        {
            int index = source.FindIndex(e => element.Entity.Transform.ZIndex < e.Entity.Transform.ZIndex);

            if (index == -1)
            {
                source.Add(element);
                return;
            }

            source.Insert(index, element);
        }

        public static void InsertElementDescending(this List<EntityNode> source, EntityNode element)
        {
            int index = source.FindLastIndex(e => e.Entity.Transform.ZIndex > element.Entity.Transform.ZIndex);
            if (index == 0 || index == -1)
            {
                source.Insert(0, element);
                return;
            }
            source.Insert(index + 1, element);
        }
    }
}
