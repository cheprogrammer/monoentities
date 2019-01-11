using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoEntities
{
    public abstract class Component
    {
        protected internal Entity Entity { get; internal set; }

        internal bool Started { get; set; } = false;

        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;

                if (!Started)
                    return;

                if (_enabled)
                    OnEnable();
                else
                    OnDisable();
            }
        }

        public bool Removed { get; internal set; } = false;

        internal bool MarkedToBeRemoved { get; set; } = false;


        internal bool Processable => Started && Enabled && !MarkedToBeRemoved;


        protected internal Component()
        {

        }

        protected internal virtual void OnEnable()
        {

        }

        protected internal virtual void Start()
        {

        }

        protected internal virtual void Update(GameTime gametime)
        {

        }

        protected internal virtual void Draw(GameTime gameTime)
        {

        }

        protected internal virtual void OnDisable()
        {

        }

        protected internal virtual void OnDestroy()
        {

        }

        public void Destroy()
        {
            Entity.RemoveComponent(GetType());
        }
    }
}
