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
    }
}
