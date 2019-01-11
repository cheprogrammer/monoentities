using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoEntities;
using NUnit.Framework;
using Tests.Components;
using Tests.Templates;

namespace Tests
{
    [TestFixture]
    public class WorkflowUnitTest
    {
        [Test]
        public void BasicWorkflow()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TestEntityTemplate>();

            ComponentA component = entity.GetComponent<ComponentA>();

            Assert.That(!component.IsStarted);
            Assert.That(!component.IsUpdated);
            Assert.That(!component.IsDrawn);
            Assert.That(!component.IsDestroyed);

            service.Update(new GameTime());
            service.Draw(new GameTime());

            Assert.That(component.IsStarted);
            Assert.That(component.IsUpdated);
            Assert.That(component.IsDrawn);
            Assert.That(!component.IsDestroyed);
        }

        [Test]
        public void BasicWorkflowWithTwoComponents()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity entity = service.CreateEntityFromTemplate<TwoComponentsEntityTemplate>();

            ComponentA componentA = entity.GetComponent<ComponentA>();
            ComponentB componentB = entity.GetComponent<ComponentB>();

            service.Update(new GameTime());

            Assert.That(componentB.ComponentAReference, Is.EqualTo(componentA));
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

            Assert.That(!entity.IsInHierarchy);

            Assert.That(service.Tree.Count(), Is.EqualTo(1));
            Assert.That(service.Tree.First(), Is.EqualTo(service.Tree));
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
            Assert.That(!entity.IsInHierarchy);

            Assert.That(service.Tree.Count(), Is.EqualTo(1));
            Assert.That(service.Tree.First(), Is.EqualTo(service.Tree));
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

            Assert.That(!entityParent.IsInHierarchy);
            Assert.That(!entityChild.IsInHierarchy);

            Assert.That(parentComponent.IsDestroyed);
            Assert.That(childComponent.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(1));
            Assert.That(service.Tree.First(), Is.EqualTo(service.Tree));
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

            Assert.That(!entityParent.IsInHierarchy);
            Assert.That(!entityChild.IsInHierarchy);

            Assert.That(parentComponent.IsDestroyed);
            Assert.That(childComponent.IsDestroyed);

            Assert.That(service.Tree.Count(), Is.EqualTo(1));
            Assert.That(service.Tree.First(), Is.EqualTo(service.Tree));
        }
    }
}

