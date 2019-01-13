using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;

namespace Tests.Components
{
    public class ConfigurableComponent : Component
    {
        public Action<ConfigurableComponent> OnStartAction;

        public Action<ConfigurableComponent> OnUpdateAction;

        public Action<ConfigurableComponent> OnDrawAction;

        public Action<ConfigurableComponent> OnDestroyAction;

        protected override void Start()
        {
            OnStartAction?.Invoke(this);
        }

        protected override void Update(GameTime gametime)
        {
            OnUpdateAction?.Invoke(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            OnDrawAction?.Invoke(this);
        }

        protected override void OnDestroy()
        {
            OnDestroyAction?.Invoke(this);
        }
    }
}
