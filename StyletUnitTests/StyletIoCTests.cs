using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    interface ISimpleTest
    {
    }

    class SimpleTest : ISimpleTest
    {
    }

    [TestFixture]
    public class StyletIoCTests
    {
        [Test]
        public void SimpleSelfTransientBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.Bind<SimpleTest>().ToSelf();
            var obj1 = ioc.Get<SimpleTest>();
            var obj2 = ioc.Get<SimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SimpleSelfSingletonBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<SimpleTest>().ToSelf();
            var obj1 = ioc.Get<SimpleTest>();
            var obj2 = ioc.Get<SimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void SimpleFactoryTransientBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.Bind<SimpleTest>().ToFactory(c => new SimpleTest());
            var obj1 = ioc.Get<SimpleTest>();
            var obj2 = ioc.Get<SimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SimpleFactorySingletonBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<SimpleTest>().ToFactory(c => new SimpleTest());
            var obj1 = ioc.Get<SimpleTest>();
            var obj2 = ioc.Get<SimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void SimpleImplementationTransientBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.Bind<ISimpleTest>().To<SimpleTest>();
            var obj1 = ioc.Get<ISimpleTest>();
            var obj2 = ioc.Get<ISimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SimpleImplementationSingletonBindingResolves()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<ISimpleTest>().To<SimpleTest>();
            var obj1 = ioc.Get<ISimpleTest>();
            var obj2 = ioc.Get<ISimpleTest>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }
    }
}
