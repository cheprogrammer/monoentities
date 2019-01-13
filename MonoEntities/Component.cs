using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoEntities
{
    public abstract class Component
    {
        public Entity Entity { get; internal set; }

        internal EcsService Service => Entity.Service;

        internal bool Started { get; set; } = false;

        public Transform2DComponent Transform => Entity.Transform;

        internal bool MarkedToBeRemoved { get; set; } = false;

        internal bool Processable => Started && !MarkedToBeRemoved && Entity.EnabledInHierarchy;

        [ExcludeFromCodeCoverage]
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

        protected internal virtual void BeforeDraw(GameTime gametime)
        {

        }

        protected internal virtual void Draw(GameTime gameTime)
        {

        }

        protected internal virtual void AfterDraw(GameTime gametime)
        {

        }

        protected internal virtual void OnDisable()
        {

        }

        protected internal virtual void OnDestroy()
        {

        }

        public virtual void Reset()
        {

        }

        public void Destroy()
        {
            Entity.RemoveComponent(GetType());
        }

        public Entity CreateEntityFromTemplate<T>(params object[] args) where T : EntityTemplate
        {
            return Service.CreateEntityFromTemplate<T>(args);
        }

        public Entity CreateEntityFromTemplate(string templateName, params object[] args)
        {
            return Service.CreateEntityFromTemplate(templateName, args);
        }

        public Entity CreateEntity()
        {
            return Service.CreateEntity();
        }
    }
}
