﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEntities;
using Tests.Components;

namespace Tests.Templates
{
    [EntityTemplate(Name = nameof(TestEntityTemplate))]
    public class TestEntityTemplate : EntityTemplate
    {
        public override void BuildEntity(Entity entity, params object[] args)
        {
            entity.AddComponent<ComponentA>();
        }
    }
}
