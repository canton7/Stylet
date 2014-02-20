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
    public class StyletIoCFactoryTests
    {
        public interface I1 { }
        public class C1 : I1 { }
        public class C12 : I1 { }

        public interface I1Factory
        {
            I1 GetI1();
        }

        public interface I1Factory2
        {
            I1 GetI1(string key = null);
        }

        public interface I1Factory3
        {
            IEnumerable<I1> GetAllI1s();
        }

        public interface IFactoryWithKeys
        {
            [Inject("Key")]
            I1 GetI1WithoutKey();

            [Inject("Key")]
            I1 GetI1WithKey(string key);
        }

        interface IPrivateFactory
        {
        }

        public interface IFactoryWithBadMethod
        {
            C1 MethodWithArgs(bool arg);
        }

        public interface IFactoryWithVoidMethod
        {
            void Method();
        }

        [Test]
        public void CreatesImplementationWithoutKey()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>();
            builder.Bind<I1Factory>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var factory = ioc.Get<I1Factory>();
            Assert.IsNotNull(factory);

            var result = factory.GetI1();
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void CreatesImplementationWithKey()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>().WithKey("key");
            builder.Bind<I1Factory2>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var factory = ioc.Get<I1Factory2>();
            Assert.IsNotNull(factory);

            var result = factory.GetI1("key");
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void CreatesAllImplementations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>();
            builder.Bind<I1>().To<C12>();
            builder.Bind<I1Factory3>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var factory = ioc.Get<I1Factory3>();
            Assert.IsNotNull(factory);

            var results = factory.GetAllI1s().ToList();

            Assert.AreEqual(2, results.Count);
            Assert.IsInstanceOf<C1>(results[0]);
            Assert.IsInstanceOf<C12>(results[1]);
        }

        [Test]
        public void UsesAttributeKeyIfKeyParameterNotGiven()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>().WithKey("Key");
            builder.Bind<I1>().To<C12>();
            builder.Bind<IFactoryWithKeys>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var factory = ioc.Get<IFactoryWithKeys>();
            var result = factory.GetI1WithoutKey();
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void UsesParameterKeyInPreferenceToAttributeKey()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>().WithKey("Key2");
            builder.Bind<I1>().To<C12>();
            builder.Bind<IFactoryWithKeys>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var factory = ioc.Get<IFactoryWithKeys>();
            var result = factory.GetI1WithKey("Key2");
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void ThrowsIfServiceTypeIsNotInterface()
        {
            var builder = new StyletIoCBuilder();
             builder.Bind<C1>().ToAbstractFactory();
            Assert.Throws<StyletIoCCreateFactoryException>(() => builder.BuildContainer());
        }

        [Test]
        public void ThrowsIfInterfaceNotPublic()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IPrivateFactory>().ToAbstractFactory();
            Assert.Throws<StyletIoCCreateFactoryException>(() => builder.BuildContainer());
        }

        [Test]
        public void ThrowsIfMethodHasArgumentOtherThanString()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IFactoryWithBadMethod>().ToAbstractFactory();
            Assert.Throws<StyletIoCCreateFactoryException>(() => builder.BuildContainer());
        }

        [Test]
        public void ThrowsIfMethodReturningVoid()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IFactoryWithVoidMethod>().ToAbstractFactory();
            Assert.Throws<StyletIoCCreateFactoryException>(() => builder.BuildContainer());
        }
    }
}
