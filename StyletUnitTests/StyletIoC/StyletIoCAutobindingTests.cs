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
    public class StyletIoCAutobindingTests
    {
        interface I1 { }
        class C11 : I1 { }
        class C12 : I1 { }
        abstract class C13 : I1 { }

        interface I2<T> { }
        class C21<T> : I2<T> { }
        class C22<T> : I2<T> { }

        [Test]
        public void NongenericInterfaceToAllImplementations()
        {
            var ioc = new StyletIoCContainer();
            ioc.Bind<I1>().ToAllImplementations();

            var result = ioc.GetAll<I1>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<C11>(result[0]);
            Assert.IsInstanceOf<C12>(result[1]);
        }

        [Test]
        public void GenericInterfaceToAllImplementations()
        {
            var ioc = new StyletIoCContainer();
            ioc.Bind(typeof(I2<>)).ToAllImplementations();
            
            var result = ioc.GetAll<I2<int>>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<C21<int>>(result[0]);
            Assert.IsInstanceOf<C22<int>>(result[1]);
        }

        [Test]
        public void IgnoresAllImplementsWhichIsNotPossible()
        {
            var ioc = new StyletIoCContainer();
            ioc.Bind<I1>().ToAllImplementations();

            var result = ioc.GetAll<I1>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsNotInstanceOf<C13>(result[0]);
            Assert.IsNotInstanceOf<C13>(result[1]);
        }
    }
}
