using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEntities;
using MonoEntities.Components;
using NUnit.Framework;
using Tests.Components;
using Tests.Templates;

namespace Tests
{
    [TestFixture]
    public class ExceptionalTests
    {
        [Test]
        public void RemoveTransformComponent()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
            
            Assert.That(() => entity.RemoveComponent<Transform2DComponent>(), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void ComponentAddingDuringDraw()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            var component = entity.GetComponent<ConfigurableComponent>();
            component.OnDrawAction = configurableComponent =>
            {
                configurableComponent.Entity.AddComponent<ComponentA>();
            };

            service.Update(new GameTime());
            Assert.That(() => service.Draw(new GameTime()), Throws.Exception.TypeOf<EcsWorkflowException>());
        }

        [Test]
        public void ComponentRemovingDuringDraw()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            var component = entity.GetComponent<ConfigurableComponent>();
            component.OnDrawAction = configurableComponent =>
            {
                configurableComponent.Destroy();
            };

            service.Update(new GameTime());
            Assert.That(() => service.Draw(new GameTime()), Throws.Exception.TypeOf<EcsWorkflowException>());
        }

        [Test]
        public void EntityAddingDuringDraw()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            var component = entity.GetComponent<ConfigurableComponent>();
            component.OnDrawAction = configurableComponent =>
                {
                    configurableComponent.CreateEntityFromTemplate<ConfigurableComponentTemplate>();
                };

            service.Update(new GameTime());
            Assert.That(() => service.Draw(new GameTime()), Throws.Exception.TypeOf<EcsWorkflowException>());
        }

        [Test]
        public void EntityDestroyDuringDraw()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();
            Entity entity = service.CreateEntityFromTemplate<ConfigurableComponentTemplate>();

            var component = entity.GetComponent<ConfigurableComponent>();
            component.OnDrawAction = configurableComponent =>
            {
                configurableComponent.Entity.Destroy();
            };

            service.Update(new GameTime());
            Assert.That(() => service.Draw(new GameTime()), Throws.Exception.TypeOf<EcsWorkflowException>());
        }

        [Test]
        public void CreateEntityFromNotExistentTemplateByType()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Assert.That(() => service.CreateEntityFromTemplate(typeof(ComponentA)), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void CreateEntityFromNotExistentTemplateByName()
        {
            EcsService service = EcsServiceFactory.CreateECSManager();

            Assert.That(() => service.CreateEntityFromTemplate("SomeNotExistentTemplateName"), Throws.Exception.TypeOf<ArgumentException>());
        }
    }
}
