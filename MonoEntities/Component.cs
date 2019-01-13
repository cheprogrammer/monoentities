using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities.Components;

namespace MonoEntities
{
    public abstract class Component
    {
        public Entity Entity { get; internal set; }

        internal EcsService Service => Entity.Service;

        internal bool Started { get; set; } = false;

        public Transform2DComponent Transform => Entity.Transform;

        internal bool Removed { get; set; } = false;

        internal bool MarkedToBeRemoved { get; set; } = false;

        internal bool Processable => Started && !MarkedToBeRemoved && Entity.EnabledInHierarchy;

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
