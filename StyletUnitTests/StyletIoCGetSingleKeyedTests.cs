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
        class C3 : IC { }

        [Inject("key1")]
        class C4 : IC { }

        [Test]
        public void GetReturnsKeyedType()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC>().To<C1>().WithKey("key1");
            ioc.Bind<IC>().To<C2>().WithKey("key2");

            Assert.IsInstanceOf<C1>(ioc.Get<IC>("key1"));
            Assert.IsInstanceOf<C2>(ioc.Get<IC>("key2"));
        } 

        [Test]
        public void GetAllReturnsKeyedTypes()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC>().To<C1>().WithKey("key1");
            ioc.Bind<IC>().To<C2>().WithKey("key1");
            ioc.Bind<IC>().To<C3>();

            var results = ioc.GetAll<IC>("key1").ToList();

            Assert.AreEqual(results.Count, 2);
            Assert.IsInstanceOf<C1>(results[0]);
            Assert.IsInstanceOf<C2>(results[1]);
        }

        [Test]
        public void AttributeIsUsed()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC>().To<C3>();
            ioc.Bind<IC>().To<C4>();

            Assert.IsInstanceOf<C4>(ioc.Get<IC>("key1"));
        }

        [Test]
        public void GivenKeyOverridesAttribute()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC>().To<C3>();
            ioc.Bind<IC>().To<C4>().WithKey("key2");

            Assert.IsInstanceOf<C4>(ioc.Get<IC>("key2"));
        }
    }
}
