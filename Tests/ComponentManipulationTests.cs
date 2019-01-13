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
        public void CreateEntityWithComponentWhichAddaAnotherComponentDuringStart()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ComponentCreatesAnotherComponentTemplate>();

            service.Update(new GameTime());

            var component = entity.GetComponent<ComponentCreatesAnotherComponent>();
            var componentA = entity.GetComponent<ComponentA>();
            var componentB = entity.GetComponent<ComponentB>();

            Assert.That(component, Is.Not.Null);
            Assert.That(componentA, Is.Not.Null);
            Assert.That(componentB, Is.Not.Null);

            Assert.That(componentA.IsStarted);
            Assert.That(componentB.IsStarted);
        }
    }
}
