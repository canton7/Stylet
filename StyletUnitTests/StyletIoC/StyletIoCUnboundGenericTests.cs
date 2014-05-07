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
    public class StyletIoCUnboundGenericTests
    {
        interface I1<T> { }
        class C1<T> : I1<T> { }

        interface I2<T, U> { }
        class C2<T, U> : I2<U, T> { }

        [Test]
        public void ResolvesSingleGenericType()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C1<>)).ToSelf();
            var ioc = builder.BuildContainer();

            Assert.DoesNotThrow(() => ioc.Get<C1<int>>());
        }

        [Test]
        public void ResolvesGenericTypeFromInterface()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I1<>)).To(typeof(C1<>));
            var ioc = builder.BuildContainer();

            var result = ioc.Get<I1<int>>();
            Assert.IsInstanceOf<C1<int>>(result);
        }

        [Test]
        public void ResolvesGenericTypeWhenOrderOfTypeParamsChanged()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I2<,>)).To(typeof(C2<,>));
            var ioc = builder.BuildContainer();

            var c2 = ioc.Get<I2<int, bool>>();
            Assert.IsInstanceOf<C2<bool, int>>(c2);
        }

        [Test]
        public void ResolvesSingletonUnboundGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I1<>)).To(typeof(C1<>)).InSingletonScope();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc.Get<I1<int>>(), ioc.Get<I1<int>>());
        }

        [Test]
        public void ResolvesUnboundGenericFromKey()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(I1<>)).To(typeof(C1<>)).WithKey("test");
            var ioc = builder.BuildContainer();

            Assert.NotNull(ioc.Get<I1<int>>("test"));
        }
    }
}
