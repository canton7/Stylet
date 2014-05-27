using NUnit.Framework;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCAutobindingTests
    {
        interface I1 { }
        class C11 : I1 { }
        class C12 : I1 { }
        abstract class C13 : I1 { }

        interface I2<T> { }
        class C21<T> : I2<T> { }
        class C22<T> : I2<T> { }


        interface I3<T> { }
        class C31 : I3<int> { }
        class C32 : I3<string> { }

        [Inject("Key")]
        class C4 { }

        [Test]
        public void NongenericInterfaceToAllImplementations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().ToAllImplementations();
            var ioc = builder.BuildContainer();

            var result = ioc.GetAll<I1>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<C11>(result[0]);
            Assert.IsInstanceOf<C12>(result[1]);
        }

        [Test]
        public void GenericInterfaceToAllImplementations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I2<>)).ToAllImplementations();
            var ioc = builder.BuildContainer();
            
            var result = ioc.GetAll<I2<int>>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<C21<int>>(result[0]);
            Assert.IsInstanceOf<C22<int>>(result[1]);
        }

        [Test]
        public void IgnoresAllImplementsWhichIsNotPossible()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().ToAllImplementations();
            var ioc = builder.BuildContainer();

            var result = ioc.GetAll<I1>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsNotInstanceOf<C13>(result[0]);
            Assert.IsNotInstanceOf<C13>(result[1]);
        }

        [Test]
        public void AutobindingBindsConcreteTypes()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind(Enumerable.Empty<Assembly>());
            var ioc = builder.BuildContainer();

            var result = ioc.Get<C11>();
            Assert.IsInstanceOf<C11>(result);
        }

        [Test]
        public void AutobindingBindsGenericTypes()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            var ioc = builder.BuildContainer();

            var result = ioc.Get<C21<int>>();
            Assert.IsInstanceOf<C21<int>>(result);
        }

        [Test]
        public void AutobindingDoesNotBindInterfaceTypes()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCRegistrationException>(() => ioc.Get<I1>());
        }

        [Test]
        public void AutobindingRespectsKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            var ioc = builder.BuildContainer();

            var result = ioc.Get<C4>("Key");
            Assert.IsInstanceOf<C4>(result);
        }

        [Test]
        public void AutobindingBindingsCanBeReplaced()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            builder.Bind<C11>().ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var result1 = ioc.Get<C11>();
            var result2 = ioc.Get<C11>();
            Assert.AreEqual(result2, result1);
        }

        [Test]
        public void BindsGenericInterfaceToAllNonGenericImplementations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I3<>)).ToAllImplementations();
            var ioc = builder.BuildContainer();

            var c31 = ioc.Get<I3<int>>();
            Assert.IsInstanceOf<C31>(c31);
        }
    }
}
