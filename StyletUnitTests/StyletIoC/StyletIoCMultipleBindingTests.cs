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

        private interface I11 { }
        private interface I12 { }

        private class C1 : I11, I12 { }

        private interface I2<T> { }
        private class C2<T> : I2<T> { }

        [Test]
        public void SingletonMultipleTypeBindingIsSingleton()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().And<C1>().To<C1>().InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I11>());
        }

        [Test]
        public void SingletonMultipleFactoryBindingIsSingleton()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().And<C1>().ToFactory(x => new C1()).InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I11>());
        }

        [Test]
        public void SingletonMultipleInstanceBindingWorks()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().And<C1>().ToInstance(new C1());
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<C1>(), ioc.Get<I11>());
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
            builder.Bind<I11>().And<I11>().To<C1>();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
        }

        [Test]
        public void AllowsMultipleBindingsWithDifferentKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().WithKey("foo").And<I11>().To<C1>().InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<I11>(), ioc.Get<I11>("foo"));
        }

        [Test]
        public void FinalWithKeyAppliesToAllBindings()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().And<C1>().To<C1>().WithKey("foo").InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.DoesNotThrow(() => ioc.Get<I11>("foo"));
            Assert.DoesNotThrow(() => ioc.Get<C1>("foo"));
            Assert.AreEqual(ioc.Get<I11>("foo"), ioc.Get<C1>("foo"));
        }

        [Test]
        public void WeakBindingsRemoveIfAnyOtherStrongBindingWithSameTypeAndKeyExists()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I11>().And<C1>().To<C1>().AsWeakBinding();
            builder.Bind<I12>().And<C1>().To<C1>();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(0, ioc.GetAll<I11>().Count());
            Assert.AreEqual(1, ioc.GetAll<C1>().Count());
            Assert.AreEqual(1, ioc.GetAll<I12>().Count());
        }
    }
}
