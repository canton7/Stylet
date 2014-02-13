using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    class C1 { }
    class C2
    {
        public C1 C1;
        public C2(C1 c1)
        {
            this.C1 = c1;
        }
    }

    class C3
    {
        public C1 C1;
        public C2 C2;
        public C3(C1 c1, C2 c2)
        {
            this.C1 = c1;
            this.C2 = c2;
        }
    }

    class C4
    {
        public C1 C1;
        public C4([Inject("key1")] C1 c1)
        {
            this.C1 = c1;
        }
    }

    [TestFixture]
    public class StyletIoCConstructorInjectionTests
    {
        [Test]
        public void RecursivelyPopulatesConstructorParams()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<C2>().ToSelf();
            ioc.Bind<C3>().ToSelf();

            var c3 = ioc.Get<C3>();

            Assert.IsInstanceOf<C3>(c3);
            Assert.IsInstanceOf<C1>(c3.C1);
            Assert.IsInstanceOf<C2>(c3.C2);
            Assert.IsInstanceOf<C1>(c3.C2.C1);
        }

        [Test]
        public void UsesConstructorParamKeys()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf("key1");
            ioc.Bind<C4>().ToSelf();

            var c4 = ioc.Get<C4>();

            Assert.IsInstanceOf<C1>(c4.C1);
        }

        [Test]
        public void ThrowsIfConstructorParamKeyNotRegistered()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C4>().ToSelf();
            ioc.Bind<C1>().ToSelf();

            Assert.Throws<StyletIoCRegistrationException>(() => ioc.Get<C4>());
        }
    }
}
