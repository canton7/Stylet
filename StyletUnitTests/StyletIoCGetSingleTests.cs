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
    public class StyletIoCGetSingleTests
    {
        interface IC1 { }
        class C1 : IC1 { }

        [Test]
        public void SelfTransientBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SelfTransientBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(C1)).ToSelf();
            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SelfSingletonBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<C1>().ToSelf();
            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void SelfSingletonBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton(typeof(C1)).ToSelf();
            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void FactoryTransientBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToFactory(c => new C1());
            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void FactoryTransientBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(C1)).ToFactory(c => new C1());
            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void FactorySingletonBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<C1>().ToFactory(c => new C1());
            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void FactorySingletonBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton(typeof(C1)).ToFactory(c => new C1());
            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ImplementationTransientBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.Bind<IC1>().To<C1>();
            var obj1 = ioc.Get<IC1>();
            var obj2 = ioc.Get<IC1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ImplementationTransientBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(IC1)).To(typeof(C1));
            var obj1 = ioc.Get(typeof(IC1));
            var obj2 = ioc.Get(typeof(IC1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ImplementationSingletonBindingResolvesGeneric()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<IC1>().To<C1>();
            var obj1 = ioc.Get<IC1>();
            var obj2 = ioc.Get<IC1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ImplementationSingletonBindingResolvesTyped()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton(typeof(IC1)).To(typeof(C1));
            var obj1 = ioc.Get(typeof(IC1));
            var obj2 = ioc.Get(typeof(IC1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }
    }
}
