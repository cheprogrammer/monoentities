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
    public class EntityManipulationTests
    {
        [Test]
        public void CreateEntityBeforeUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();

            Assert.That(entity.Transform, Is.Not.Null);

            ComponentA component = entity.GetComponent<ComponentA>();

            Assert.That(component.Transform, Is.Not.Null);

            Assert.That(!component.IsStarted);
            Assert.That(!component.IsUpdated);
            Assert.That(!component.IsDrew);
            Assert.That(!component.IsDestroyed);

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(component.IsStarted);
            Assert.That(component.IsUpdated);
            Assert.That(component.IsDrew);
            Assert.That(!component.IsDestroyed);
        }

        [Test]
        public void CreateEntityWithTwoComponents()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();

            ComponentA componentA = entity.GetComponent<ComponentA>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            service.Update(new GameTime());

            Assert.That(componentB.ComponentAReference, Is.EqualTo(componentA));
        }

        [Test]
        public void DestoryEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();
            ComponentB componentA = entity.GetComponent<ComponentB>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            service.Update(new GameTime());

            entity.Destroy();

            service.Update(new GameTime());

            Assert.That(componentA.IsStarted);
            Assert.That(componentA.IsUpdated);
            Assert.That(componentA.IsDestroyed);

            Assert.That(componentB.IsStarted);
            Assert.That(componentB.IsUpdated);
            Assert.That(componentB.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DestoryEntityBeforeAdding()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();
            ComponentB componentA = entity.GetComponent<ComponentB>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            entity.Destroy();

            service.Update(new GameTime());

            Assert.That(!componentA.IsStarted);
            Assert.That(!componentA.IsUpdated);
            Assert.That(!componentB.IsStarted);
            Assert.That(!componentB.IsUpdated);

            Assert.That(componentA.IsDestroyed);
            Assert.That(componentB.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DestoryEntityWithChildren()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entityParent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();
            entityChild.Transform.Parent = entityParent.Transform;

            ComponentA parentComponent = entityParent.GetComponent<ComponentA>();
            ComponentA childComponent = entityChild.GetComponent<ComponentA>();

            service.Update(new GameTime());

            entityParent.Destroy();

            service.Update(new GameTime());

            Assert.That(parentComponent.IsDestroyed);
            Assert.That(childComponent.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DestoryEntityWithChildrenBeforeAdding()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entityParent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();
            entityChild.Transform.Parent = entityParent.Transform;

            ComponentA parentComponent = entityParent.GetComponent<ComponentA>();
            ComponentA childComponent = entityChild.GetComponent<ComponentA>();

            entityParent.Destroy();

            service.Update(new GameTime());

            Assert.That(parentComponent.IsDestroyed);
            Assert.That(childComponent.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateEntityDuringStart()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();
            Entity newEntity = null;

            component.OnStartAction = configurableComponent =>
            {
                newEntity = service.CreateEntity();
                newEntity.AddComponent<ConfigurableComponent>();
            };

            service.Update(new GameTime());

            Assert.That(component, Is.Not.Null);
            Assert.That(newEntity, Is.Not.Null);
            Assert.That(newEntity.GetComponent<ConfigurableComponent>(), Is.Not.Null);
        }

        [Test]
        public void CreateEntityDuringStartFromNamedTemplate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();
            Entity newEntity = null;

            component.OnStartAction = configurableComponent =>
                {
                    newEntity = service.CreateEntityFromTemplate(nameof(ConfigurableComponentTemplate));
                };

            service.Update(new GameTime());

            Assert.That(component, Is.Not.Null);
            Assert.That(newEntity, Is.Not.Null);
            Assert.That(newEntity.GetComponent<ConfigurableComponent>(), Is.Not.Null);
        }

        [Test]
        public void CreateEntityDuringUpdateFromTemplate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();
            Entity newEntity = null;

            component.OnUpdateAction = configurableComponent =>
                {
                    newEntity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
                };

            service.Update(new GameTime());

            Assert.That(component, Is.Not.Null);
            Assert.That(newEntity, Is.Not.Null);
            Assert.That(newEntity.GetComponent<ConfigurableComponent>(), Is.Not.Null);
        }

        [Test]
        public void DestroyEntityDuringStart()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();

            component.OnStartAction = configurableComponent =>
            {
                configurableComponent.Entity.Destroy();
            };

            service.Update(new GameTime());

            Assert.That(service.Tree, Is.Empty);
        }

        [Test]
        public void DestroyEntityDuringUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();

            component.OnUpdateAction = configurableComponent =>
            {
                configurableComponent.Entity.Destroy();
            };

            service.Update(new GameTime());

            Assert.That(service.Tree, Is.Empty);
        }

        [Test]
        public void DoubleDestroyEntity()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            service.Update(new GameTime());

            entity.Destroy();
            Assert.That(() => entity.Destroy(), Throws.Exception.TypeOf<EcsWorkflowException>());
        }
    }
}
