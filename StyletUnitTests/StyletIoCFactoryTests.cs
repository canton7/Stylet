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
    public class StyletIoCFactoryTests
    {
        public interface I1 { }
        public class C1 : I1 { }

        public interface I1Factory
        {
            I1 GetI1();
        }

        public interface I1Factory2
        {
            I1 GetI1(string key = null);
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
            var ioc = new StyletIoC();
            ioc.Bind<I1>().To<C1>();
            ioc.Bind<I1Factory>().ToAbstractFactory();

            var factory = ioc.Get<I1Factory>();
            var result = factory.GetI1();
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void CreatesImplementationWithKey()
        {
            var ioc = new StyletIoC();
            ioc.Bind<I1>().To<C1>("key");
            ioc.Bind<I1Factory2>().ToAbstractFactory();

            var factory = ioc.Get<I1Factory2>();
            var result = factory.GetI1("key");
            Assert.IsInstanceOf<C1>(result);
        }

        [Test]
        public void ThrowsIfServiceTypeIsNotInterface()
        {
            var ioc = new StyletIoC();
            Assert.Throws<StyletIoCCreateFactoryException>(() => ioc.Bind<C1>().ToAbstractFactory());
        }

        [Test]
        public void ThrowsIfInterfaceNotPublic()
        {
            var ioc = new StyletIoC();
            Assert.Throws<StyletIoCCreateFactoryException>(() => ioc.Bind<IPrivateFactory>().ToAbstractFactory());
        }

        [Test]
        public void ThrowsIfMethodHasArgumentOtherThanString()
        {
            var ioc = new StyletIoC();
            Assert.Throws<StyletIoCCreateFactoryException>(() => ioc.Bind<IFactoryWithBadMethod>().ToAbstractFactory());
        }

        [Test]
        public void ThrowsIfMethodReturningVoid()
        {
            var ioc = new StyletIoC();
            Assert.Throws<StyletIoCCreateFactoryException>(() => ioc.Bind<IFactoryWithVoidMethod>().ToAbstractFactory());
        }
    }
}
