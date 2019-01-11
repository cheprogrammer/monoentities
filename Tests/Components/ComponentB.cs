using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;

namespace Tests.Components
{
    public class ComponentB : ComponentA
    {

        public ComponentA ComponentAReference { get; set; }

        protected override void Start()
        {
            base.Start();

            ComponentAReference = Entity.GetComponent<ComponentA>();
        }
    }
}
