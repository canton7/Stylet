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
    public class StyletIoCBindingChecksTests
    {
        interface I1 { }
        class C2 { }
        interface I3 : I1 { }
        abstract class C4 : I1 { }
        class C5<T> : I1 { }
        
        interface I6<T> { }
        class C6<T> : I6<T> { }
        interface I7<T, U> { }
        class C7<T, U> { }

        [Test]
        public void ThrowsIfTypeDoesNotImplementService()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind<I1>().To<C2>());
        }

        [Test]
        public void ThrowsIfImplementationIsNotConcrete()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind<I1>().To<I3>());
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind<I1>().To<C4>());
        }

        [Test]
        public void ThrowsIfImplementationIsSingletonUnboundGeneric()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind<I1>().To(typeof(C5<>)).InSingletonScope());
        }

        [Test]
        public void ThrowsIfUnboundGenericServiceBoundToNormalImplementation()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind(typeof(I6<>)).To<C6<int>>());
        }

        [Test]
        public void ThrowsIfNormalServiceBoundToUnboundGenericService()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind<I6<int>>().To(typeof(C6<>)));
        }

        [Test]
        public void ThrowsIfUnboundTypesHaveDifferentNumbersOfTypeParameters()
        {
            var builder = new StyletIoCBuilder();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind(typeof(I6<>)).To(typeof(C7<,>)));
            Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind(typeof(I7<,>)).To(typeof(C6<>)));
        }
    }
}
