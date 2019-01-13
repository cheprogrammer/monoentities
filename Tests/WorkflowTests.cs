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
    public class WorkflowTests
    {
        [Test]
        public void CheckIdsAddingAndRemoving()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            int entitesCount = 10;

            Entity[] createdEntities = new Entity[entitesCount];
            for (int i = 0; i < entitesCount; i++)
            {
                createdEntities[i] = service.CreateEntityFromTemplate<TestEntityTemplate>();
            }

            for (int i = 0; i < entitesCount; i++)
            {
                Assert.That(createdEntities[i].Id, Is.EqualTo(i+1));
            }

            service.Update(new GameTime());

            createdEntities[1].Destroy();

            service.Update(new GameTime());

            var newEntityWithOldId = service.CreateEntityFromTemplate<TestEntityTemplate>();

            Assert.That(newEntityWithOldId.Id, Is.EqualTo(2));
        }
    }
}

