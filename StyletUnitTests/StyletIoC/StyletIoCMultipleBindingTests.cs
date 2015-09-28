using NUnit.Framework;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCMultipleBindingTests
    {

        private interface I1 { }

        private class C1 : I1 { }

        private interface I2<T> { }
        private class C2<T> : I2<T> { }

        [Test]
        public void SingletonMultipleTypeBindingIsSingleton()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().And<C1>().To<C1>().InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I1>());
        }

        [Test]
        public void SingletonMultipleFactoryBindingIsSingleton()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().And<C1>().ToFactory(x => new C1()).InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I1>());
        }

        [Test]
        public void SingletonMultipleInstanceBindingWorks()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().And<C1>().ToInstance(new C1());
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I1>());
        }

        [Test]
        public void RejectsMultipleUnboundGenericBindings()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I2<>)).And(typeof(C2<>)).To(typeof(C2<>)).InSingletonScope();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
        }

        [Test]
        public void RejectsMultipleBindingsForTheSameType()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().And<I1>().To<C1>();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
        }

        [Test]
        public void AllowsMultipleBindingsWithDifferentKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().WithKey("foo").And<I1>().To<C1>().InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<I1>(), ioc.Get<I1>("foo"));
        }

        [Test]
        public void FinalWithKeyAppliesToAllBindings()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().And<C1>().To<C1>().WithKey("foo").InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.DoesNotThrow(() => ioc.Get<I1>("foo"));
            Assert.DoesNotThrow(() => ioc.Get<C1>("foo"));
            Assert.AreEqual(ioc.Get<I1>("foo"), ioc.Get<C1>("foo"));
        }
    }
}
