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
    public class ComponentManipulationTests
    {
        [Test]
        public void CreateEntityWithComponentWhichRemovesItselfOnStart()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.GetComponent<ConfigurableComponent>();

            component.OnStartAction = configurableComponent =>
            {
                configurableComponent.Destroy();
            };

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.GetComponent<ConfigurableComponent>(), Is.Null);
        }

        [Test]
        public void CreateEntityWithComponentWhichRemovesItselfOnUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.GetComponent<ConfigurableComponent>();

            component.OnUpdateAction = configurableComponent =>
            {
                configurableComponent.Destroy();
            };

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(entity.GetComponent<ConfigurableComponent>(), Is.Null);
        }

        [Test]
        public void CreateEntityWithComponentWhichAddedAnotherComponentDuringStart()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.GetComponent<ConfigurableComponent>();

            component.OnStartAction = configurableComponent =>
            {
                configurableComponent.Entity.AddComponent<ComponentA>();
            };

            service.Update(new GameTime());
            service.Draw(new GameTime());

            var componentA = entity.GetComponent<ComponentA>();

            Assert.That(componentA, Is.Not.Null);
            Assert.That(componentA.IsStarted, Is.True);
        }

        [Test]
        public void CreateEntityWithComponentWhichAddedAnotherComponentDuringUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.GetComponent<ConfigurableComponent>();

            component.OnUpdateAction = configurableComponent =>
            {
                if(configurableComponent.Entity.GetComponent<ComponentA>() == null)
                    configurableComponent.Entity.AddComponent<ComponentA>();
            };

            service.Update(new GameTime());

            var componentA = entity.GetComponent<ComponentA>();

            Assert.That(componentA, Is.Not.Null);
            Assert.That(componentA.IsStarted, Is.False);

            service.Update(new GameTime());

            Assert.That(componentA.IsStarted, Is.True);
            Assert.That(componentA.IsUpdated, Is.True);
        }

        [Test]
        public void CreateEntityWithComponentWhichAddedAnotherComponentDuringStartAndRemovedDuringUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.GetComponent<ConfigurableComponent>();
            ComponentA componentA = null;

            component.OnStartAction = configurableComponent =>
            {
                componentA = configurableComponent.Entity.AddComponent<ComponentA>();
            };

            component.OnUpdateAction = configurableComponent =>
            {
                configurableComponent.Entity.RemoveComponent<ComponentA>();
            };

            service.Update(new GameTime());

            Assert.That(entity.GetComponent<ComponentA>(), Is.Null);
            Assert.That(componentA.IsStarted, Is.True);
            Assert.That(componentA.IsUpdated, Is.False);
        }

        [Test]
        public void RemoveComponent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            Assert.That(!componentB.IsEnabled);

            service.Update(new GameTime());

            Assert.That(componentB.IsEnabled);

            entity.RemoveComponent<ComponentB>();

            service.Update(new GameTime());

            Assert.That(componentB.IsDestroyed);
            Assert.That(!componentB.IsEnabled);
        }

        [Test]
        public void RemoveComponentBeforeUpdate()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            Assert.That(!componentB.IsEnabled);

            entity.RemoveComponent<ComponentB>();

            service.Update(new GameTime());

            Assert.That(!componentB.IsStarted);
            Assert.That(!componentB.IsUpdated);
            Assert.That(!componentB.IsEnabled);
            Assert.That(componentB.IsDestroyed);
        }

        [Test]
        public void DoubleRemoveComponent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();
            var componentA = entity.GetComponent<ComponentA>();

            service.Update(new GameTime());

            componentA.Destroy();
            Assert.That(() => componentA.Destroy(), Throws.Exception.TypeOf<EcsWorkflowException>());
        }

        [Test]
        public void FindExistentComponent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ConfigurableComponent>();

            Assert.That(component, Is.Not.Null);
        }

        [Test]
        public void FindExistentComponents()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var components = entity.FindComponents<ConfigurableComponent>();

            Assert.That(components, Has.Exactly(1).TypeOf<ConfigurableComponent>());
        }

        [Test]
        public void FindNonExistentComponent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var component = entity.FindComponent<ComponentA>();

            Assert.That(component, Is.Null);
        }

        [Test]
        public void FindNonExistentComponents()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            var components = entity.FindComponents<ComponentA>();

            Assert.That(components, Is.Empty);
        }

        [Test]
        public void FindComponentFromChildAndParent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            Assert.That(entity.FindComponent<ConfigurableComponent>(), Is.Not.Null);
            Assert.That(entityChild.FindComponent<ConfigurableComponent>(), Is.Not.Null);

            Assert.That(entity.FindComponent<ComponentA>(), Is.Not.Null);
            Assert.That(entityChild.FindComponent<ComponentA>(), Is.Not.Null);
        }

        [Test]
        public void GetComponentInParentAndChild()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            Assert.That(entityChild.GetComponentInParent<ConfigurableComponent>(), Is.Not.Null);
            Assert.That(entity.GetComponentInChild<ConfigurableComponent>(), Is.Not.Null);
        }

        [Test]
        public void GetComponentsInParentAndChild()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            Entity entityChild = service.CreateEntityFromTemplate<TestEntityTemplate>();

            entityChild.Transform.Parent = entity.Transform;

            var parentComponents = entityChild.GetComponentsInParent<ConfigurableComponent>();
            var childComponents = entity.GetComponentsInChild<ComponentA>();

            Assert.That(parentComponents, Has.Exactly(1).Items);
            Assert.That(parentComponents, Has.Exactly(1).TypeOf<ConfigurableComponent>());

            Assert.That(childComponents, Has.Exactly(1).Items);
            Assert.That(childComponents, Has.Exactly(1).TypeOf<ComponentA>());
        }
    }
}
