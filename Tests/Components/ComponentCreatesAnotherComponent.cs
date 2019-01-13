using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;

namespace Tests.Components
{
    public class ComponentCreatesAnotherComponent : Component
    {
        public ComponentA ComponentAReference;

        public ComponentB ComponentBReference;

        protected override void Start()
        {
            ComponentAReference = Entity.AddComponent<ComponentA>();
            ComponentBReference = Entity.AddComponent<ComponentB>();
        }

        protected override void Update(GameTime gametime)
        {
            
        }
    }
}
