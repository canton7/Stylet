using NUnit.Framework;
using StyletIoC;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletUnitTests.StyletIoC
{
    [TestFixture]
    public class StyletIoCAutobindingTests
    {
        private interface I1 { }

        private class C11 : I1 { }

        private class C12 : I1 { }

        private abstract class C13 : I1 { }

        private interface I2<T> { }

        private class C21<T> : I2<T> { }

        private class C22<T> : I2<T> { }

        private interface I3<T> { }

        private class C31 : I3<int> { }

        private class C32 : I3<string> { }

        [Inject("Key")]
        private class C4 { }

        private interface I5 { }

        private class C5 { }

        [Test]
        public void NongenericInterfaceToAllImplementations()
        {
            var builder = new StyletIoCBuilder();

            builder.Bind<I1>().ToAllImplementations();
            IContainer ioc = builder.BuildContainer();

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
            IContainer ioc = builder.BuildContainer();
            
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
            IContainer ioc = builder.BuildContainer();

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
            IContainer ioc = builder.BuildContainer();

            C11 result = ioc.Get<C11>();
            Assert.IsInstanceOf<C11>(result);
        }

        [Test]
        public void AutobindingBindsGenericTypes()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            IContainer ioc = builder.BuildContainer();

            C21<int> result = ioc.Get<C21<int>>();
            Assert.IsInstanceOf<C21<int>>(result);
        }

        [Test]
        public void AutobindingDoesNotBindInterfaceTypes()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            IContainer ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCRegistrationException>(() => ioc.Get<I1>());
        }

        [Test]
        public void AutobindingRespectsKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            IContainer ioc = builder.BuildContainer();

            C4 result = ioc.Get<C4>("Key");
            Assert.IsInstanceOf<C4>(result);
        }

        [Test]
        public void AutobindingBindingsCanBeReplaced()
        {
            var builder = new StyletIoCBuilder();
            builder.Autobind();
            builder.Bind<C11>().ToSelf().InSingletonScope();
            IContainer ioc = builder.BuildContainer();

            C11 result1 = ioc.Get<C11>();
            C11 result2 = ioc.Get<C11>();
            Assert.AreEqual(result2, result1);
        }

        [Test]
        public void BindsGenericInterfaceToAllNonGenericImplementations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I3<>)).ToAllImplementations();
            IContainer ioc = builder.BuildContainer();

            I3<int> c31 = ioc.Get<I3<int>>();
            Assert.IsInstanceOf<C31>(c31);
        }

        [Test]
        public void ToAllImplementationsEnumerableWithNoAssembliesLooksInCallingAssembly()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().ToAllImplementations((IEnumerable<Assembly>)null);
            IContainer ioc = builder.BuildContainer();

            var results = ioc.GetAll<I1>().ToList();
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void AutoBindWithNoAssembliesThrows()
        {
            var builder = new StyletIoCBuilder();
            builder.Assemblies = null;
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Autobind());
        }

        [Test]
        public void AutobindSearchesPassedAssemblie()
        {
            var builder = new StyletIoCBuilder();
            builder.Assemblies = null;
            builder.Autobind(typeof(StyletIoCAutobindingTests).Assembly);
            IContainer ioc = builder.BuildContainer();

            Assert.IsInstanceOf<C11>(ioc.Get<C11>());
        }

        [Test]
        public void ToAllImplementationsThrowsIfNoImplementationsFound()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I5>().ToAllImplementations();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
        }

        [Test]
        public void ToAllImplementationsDoesNotThrowIfNoImplementationsFoundAndAllowZeroImplementationsIsTrue()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I5>().ToAllImplementations(allowZeroImplementations: true);
            IContainer ioc = null;
            Assert.DoesNotThrow(() => ioc = builder.BuildContainer());

            Assert.DoesNotThrow(() => ioc.GetAll<I5>());
        }
    }
}
