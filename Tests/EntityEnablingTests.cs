using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;
using NUnit.Framework;
using Tests.Components;
using Tests.Templates;

namespace Tests
{
    [TestFixture]
    public class EntityEnablingTests
    {
        [Test]
        public void UpdateDisabledEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            ComponentA component = entity.GetComponent<ComponentA>();

            entity.Enabled = false;

            service.Update(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.False);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsUpdated, Is.False);
        }

        [Test]
        public void UpdateDisabledHierarchicalEntities()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            ComponentA component = entity.GetComponent<ComponentA>();
            ComponentA componentChild = entityChild.GetComponent<ComponentA>();

            entity.Enabled = false;

            service.Update(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.False);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsUpdated, Is.False);

            Assert.That(entityChild.EnabledInHierarchy, Is.False);
            Assert.That(componentChild.IsStarted, Is.True);
            Assert.That(componentChild.IsUpdated, Is.False);
        }

        [Test]
        public void DrawDisabledEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            ComponentA component = entity.GetComponent<ComponentA>();

            entity.Enabled = false;

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.False);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsDrew, Is.False);
        }

        [Test]
        public void DrawDisabledHierarchicalEntities()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            ComponentA component = entity.GetComponent<ComponentA>();
            ComponentA componentChild = entityChild.GetComponent<ComponentA>();

            entity.Enabled = false;

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.False);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsDrew, Is.False);

            Assert.That(entityChild.EnabledInHierarchy, Is.False);
            Assert.That(componentChild.IsStarted, Is.True);
            Assert.That(componentChild.IsDrew, Is.False);
        }

        [Test]
        public void UpdateDisabledChildlEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            ComponentA component = entity.GetComponent<ComponentA>();
            ComponentA componentChild = entityChild.GetComponent<ComponentA>();

            entityChild.Enabled = false;

            service.Update(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.True);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsUpdated, Is.True);

            Assert.That(entityChild.EnabledInHierarchy, Is.False);
            Assert.That(componentChild.IsStarted, Is.True);
            Assert.That(componentChild.IsUpdated, Is.False);
        }

        [Test]
        public void DrawDisabledChildEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            ComponentA component = entity.GetComponent<ComponentA>();
            ComponentA componentChild = entityChild.GetComponent<ComponentA>();

            entityChild.Enabled = false;

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.True);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsDrew, Is.True);

            Assert.That(entityChild.EnabledInHierarchy, Is.False);
            Assert.That(componentChild.IsStarted, Is.True);
            Assert.That(componentChild.IsDrew, Is.False);
        }

        [Test]
        public void DrawEntityWhenChildChangedEnableness()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            var component = entity.GetComponent<ComponentA>();
            var componentChild = entityChild.GetComponent<ConfigurableComponent>();

            componentChild.OnDrawAction = configurableComponent =>
                {
                    configurableComponent.Entity.Transform.Parent.Entity.Enabled = false;
                };

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.EnabledInHierarchy, Is.False);
            Assert.That(component.IsStarted, Is.True);
            Assert.That(component.IsDrew, Is.False);
        }
    }
}
