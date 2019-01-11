using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEntities;
using NUnit.Framework;

namespace Tests
{
    [SetUpFixture]
    public class SetupEcsServiceFactory
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            EcsServiceFactory.ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {

        }
    }
}
