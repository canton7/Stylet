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
    public class StyletIoCUnboundGenericTests
    {
        interface I1<T> { }
        class C1<T> : I1<T> { }

        interface I2<T, U> { }
        class C2<T, U> : I2<U, T> { }

        [Test]
        public void ResolvesSingleGenericType()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(C1<>)).ToSelf();

            Assert.DoesNotThrow(() => ioc.Get<C1<int>>());
        }

        [Test]
        public void ResolvesGenericTypeFromInterface()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(I1<>)).To(typeof(C1<>));

            var result = ioc.Get<I1<int>>();
            Assert.IsInstanceOf<C1<int>>(result);
        }

        [Test]
        public void ResolvesGenericTypeWhenOrderOfTypeParamsChanged()
        {
            var ioc = new StyletIoC();
            ioc.Bind(typeof(I2<,>)).To(typeof(C2<,>));

            var c2 = ioc.Get<I2<int, bool>>();
            Assert.IsInstanceOf<C2<bool, int>>(c2);
        }
    }
}
