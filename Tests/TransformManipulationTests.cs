using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;
using MonoEntities.Tree;
using NUnit.Framework;
using Tests.Components;
using Tests.Templates;

namespace Tests
{
    [TestFixture]
    public class TransformManipulationTests
    {
        [Test]
        public void ParentChangingBeforeRegistration()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Entity parent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity child = service.CreateEntityFromTemplate<TestEntityTemplate>();

            child.Transform.Parent = parent.Transform;

            service.Update(new GameTime());

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(parent.Transform.Parent, Is.Null);
                }

                if (entityNode.Entity == child)
                {
                    Assert.That(child.Transform.Parent, Is.EqualTo(parent.Transform));
                }
            }
        }

        [Test]
        public void ParentChangingAfterRegistration()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity parent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity child = service.CreateEntityFromTemplate<TestEntityTemplate>();

            service.Update(new GameTime());

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(parent.Transform.Parent, Is.Null);
                }

                if (entityNode.Entity == child)
                {
                    Assert.That(child.Transform.Parent, Is.Null);
                }
            }

            child.Transform.Parent = parent.Transform;
            service.Update(new GameTime());

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(parent.Transform.Parent, Is.Null);
                }

                if (entityNode.Entity == child)
                {
                    Assert.That(child.Transform.Parent, Is.EqualTo(parent.Transform));
                }
            }
        }

        [Test]
        public void ParentChangingTwiceAfterRegistration()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity parent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity child = service.CreateEntityFromTemplate<TestEntityTemplate>();

            service.Update(new GameTime());

            child.Transform.Parent = parent.Transform;
            service.Update(new GameTime());

            child.Transform.Parent = null;
            service.Update(new GameTime());

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(parent.Transform.Parent, Is.Null);
                }

                if (entityNode.Entity == child)
                {
                    Assert.That(child.Transform.Parent, Is.Null);
                }
            }
        }

        [Test]
        public void TransformChildIterationTest()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity parent = service.CreateEntityFromTemplate<TestEntityTemplate>();

            Entity child1 = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity child2 = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity child3 = service.CreateEntityFromTemplate<TestEntityTemplate>();

            child1.Transform.Parent = parent.Transform;
            child2.Transform.Parent = parent.Transform;
            child3.Transform.Parent = parent.Transform;

            Transform2DComponent[] childTransforms = new[]
            {
                child1.Transform,
                child2.Transform,
                child3.Transform
            };
            
            service.Update(new GameTime());

            Assert.That(parent.Transform, Is.EquivalentTo(childTransforms));
        }

        [Test]
        public void ZIndexChanging()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity parent = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity childOne = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity childTwo = service.CreateEntityFromTemplate<TestEntityTemplate>();
            Entity childThree = service.CreateEntityFromTemplate<TestEntityTemplate>();

            childOne.Transform.Parent = parent.Transform;
            childTwo.Transform.Parent = parent.Transform;
            childThree.Transform.Parent = parent.Transform;


            childOne.Transform.ZIndex = 10;
            childTwo.Transform.ZIndex = 20;
            childThree.Transform.ZIndex = 30;

            service.Update(new GameTime());

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(entityNode, Is.Ordered.Using(new EntityNodeZIndexComparator()));
                }
            }

            childOne.Transform.ZIndex = 20;
            childTwo.Transform.ZIndex = 10;
            childThree.Transform.ZIndex = 5;

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(entityNode, Is.Ordered.Using(new EntityNodeZIndexComparator()));
                }
            }

            childOne.Transform.ZIndex = 20;
            childTwo.Transform.ZIndex = 5;
            childThree.Transform.ZIndex = 10;

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(entityNode, Is.Ordered.Using(new EntityNodeZIndexComparator()));
                }
            }

            childOne.Transform.ZIndex = 5;
            childTwo.Transform.ZIndex = 5;
            childThree.Transform.ZIndex = 10;

            foreach (EntityNode entityNode in service.Tree)
            {
                if (entityNode.Entity == parent)
                {
                    Assert.That(entityNode, Is.Ordered.Using(new EntityNodeZIndexComparator()));
                }
            }
        }
    }
}

