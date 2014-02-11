using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCGetSingleKeyedTests
    {
        interface IC { }
        class C1 : IC { }
        class C2 : IC { }

        [Test]
        public void TestReturnsKeyedType()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC>().To<C1>("key1");
            ioc.Bind<IC>().To<C2>("key2");

            Assert.IsInstanceOf<C1>(ioc.Get<IC>("key1"));
            Assert.IsInstanceOf<C2>(ioc.Get<IC>("key2"));
        }
    }
}
