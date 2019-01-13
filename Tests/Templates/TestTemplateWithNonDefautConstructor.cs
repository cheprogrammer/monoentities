using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEntities;

namespace Tests.Templates
{
    [EntityTemplate(Name = nameof(TestTemplateWithNonDefautConstructor))]
    public class TestTemplateWithNonDefautConstructor : EntityTemplate
    {

        public TestTemplateWithNonDefautConstructor(object parameter)
        {

        }

        public override void BuildEntity(Entity entity, params object[] args)
        {
            
        }
    }
}
