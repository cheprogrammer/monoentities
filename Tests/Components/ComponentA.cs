using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;

namespace Tests.Components
{
    public class ComponentA : Component
    {
        public bool IsStarted;

        public bool IsUpdated;

        public bool IsDrew;

        public bool IsDestroyed;

        public bool IsEnabled;

        protected override void Start()
        {
            IsStarted = true;
        }

        protected override void Update(GameTime gametime)
        {
            IsUpdated = true;
        }

        protected override void Draw(GameTime gameTime)
        {
            IsDrew = true;
        }

        protected override void OnDestroy()
        {
            IsDestroyed = true;
        }

        protected override void OnEnable()
        {
            IsEnabled = true;
        }

        protected override void OnDisable()
        {
            IsEnabled = false;
        }
    }
}
