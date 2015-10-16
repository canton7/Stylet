using NUnit.Framework;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCFuncFactoryTests
    {
        private class C1 { }
        private class C2
        {
            public Func<C1> C1Func;
            public C2(Func<C1> c1Func)
            {
                this.C1Func = c1Func;
            }
        }
        public interface I1 { }
        private class C11 : I1 { }
        private class C12 : I1 { }

        public interface I1Factory
        {
            I1 GetI1();
        }

        [Test]
        public void FuncFactoryWorksForGetNoKey()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            var func = ioc.Get<Func<C1>>();
            var result = func();
            Assert.IsNotNull(result);
        }

        [Test]
        public void FuncFactoryWorksConstructorInjection()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C2>().ToSelf();
            var ioc = builder.BuildContainer();

            var c2 = ioc.Get<C2>();
            var c1Func = c2.C1Func;
            Assert.IsNotNull(c1Func());
        }

        [Test]
        public void FuncFactoryOfTransientWorksAsExpected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            var func = ioc.Get<Func<C1>>();
            Assert.AreNotEqual(func(), func());
        }

        [Test]
        public void FuncFactoryOfSingletonWorksAsExpected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var func = ioc.Get<Func<C1>>();
            Assert.AreEqual(func(), func());
        }

        [Test]
        public void FuncFactoryOfIEnumerableWorksAsExpected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C11>();
            builder.Bind<I1>().To<C12>();
            var ioc = builder.BuildContainer();

            var func = ioc.Get<Func<IEnumerable<I1>>>();
            var results = func().ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsInstanceOf<C11>(results[0]);
            Assert.IsInstanceOf<C12>(results[1]);
        }

        [Test]
        public void IEnumerableOfFuncFactoryWorksAsExpected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C11>();
            builder.Bind<I1>().To<C12>();
            var ioc = builder.BuildContainer();

            var funcCollection = ioc.GetTypeOrAll<IEnumerable<Func<I1>>>().ToList();
            var result = funcCollection[0]();

            Assert.AreEqual(2, funcCollection.Count);
            Assert.IsInstanceOf<C11>(funcCollection[0]());
            Assert.IsInstanceOf<C12>(funcCollection[1]());
        }

        [Test]
        public void FuncFactoryOfAbstractFactoryWorksAsExpected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C11>();
            builder.Bind<I1Factory>().ToAbstractFactory();
            var ioc = builder.BuildContainer();

            var func = ioc.Get<Func<I1Factory>>();
            Assert.IsNotNull(func);
            var i1 = func().GetI1();
            Assert.IsInstanceOf<C11>(i1);
        }
    }
}
