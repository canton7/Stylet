using NUnit.Framework;
using StyletIoC;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCConstructorInjectionTests
    {
        interface I1 { }

        class C1 : I1 { }
        class C2 : I1
        {
            public C1 C1;
            public C2(C1 c1)
            {
                this.C1 = c1;
            }
        }

        class C3
        {
            public C1 C1;
            public C2 C2;
            public C3(C1 c1, C2 c2)
            {
                this.C1 = c1;
                this.C2 = c2;
            }
        }

        class C4
        {
            public C1 C1;
            public C4([Inject("key1")] C1 c1)
            {
                this.C1 = c1;
            }
        }

        class C5
        {
            public bool RightConstructorCalled;
            public C5(C1 c1, C2 c2 = null, C3 c3 = null, C4 c4 = null)
            {
            }

            public C5(C1 c1, C2 c2, C3 c3 = null)
            {
                this.RightConstructorCalled = true;
            }

            public C5(C1 c1, C2 c2)
            {
            }
        }

        class C6
        {
            public bool RightConstructorCalled;
            [Inject]
            public C6(C1 c1)
            {
                this.RightConstructorCalled = true;
            }

            public C6(C1 c1, C2 c2)
            {
            }
        }

        class C7
        {
            [Inject]
            public C7()
            {
            }

            [Inject]
            public C7(C1 c1)
            {
            }
        }

        class C8
        {
            public IEnumerable<I1> I1s;
            public C8(IEnumerable<I1> i1s)
            {
                this.I1s = i1s;
            }
        }

        class C9
        {
            public C9(I1 i1)
            {
            }
        }

        class C10
        {
            public C10(ObservableCollection<C10> c1s)
            {
            }
        }

        class C11
        {
            public C11(C11 other)
            {
                throw new Exception("Wrong constructor!");
            }
        }

        [Test]
        public void RecursivelyPopulatesConstructorParams()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C2>().ToSelf();
            builder.Bind<C3>().ToSelf();
            var ioc = builder.BuildContainer(); 

            var c3 = ioc.Get<C3>();

            Assert.IsInstanceOf<C3>(c3);
            Assert.IsInstanceOf<C1>(c3.C1);
            Assert.IsInstanceOf<C2>(c3.C2);
            Assert.IsInstanceOf<C1>(c3.C2.C1);
        }

        [Test]
        public void UsesConstructorParamKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().WithKey("key1");
            builder.Bind<C4>().ToSelf();
            var ioc = builder.BuildContainer();

            var c4 = ioc.Get<C4>();

            Assert.IsInstanceOf<C1>(c4.C1);
        }

        [Test]
        public void ThrowsIfConstructorParamKeyNotRegistered()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C4>().ToSelf();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C4>());
        }

        [Test]
        public void ChoosesCtorWithMostParamsWeCanFulfill()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C2>().ToSelf();
            builder.Bind<C5>().ToSelf();
            var ioc = builder.BuildContainer();

            var c5 = ioc.Get<C5>();
            Assert.IsTrue(c5.RightConstructorCalled);
        }

        [Test]
        public void ChoosesCtorWithAttribute()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C2>().ToSelf();
            builder.Bind<C6>().ToSelf();
            var ioc = builder.BuildContainer();

            var c6 = ioc.Get<C6>();
            Assert.IsTrue(c6.RightConstructorCalled);
        }

        [Test]
        public void ThrowsIfMoreThanOneCtorWithAttribute()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C7>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C7>());
        }

        [Test]
        public void ThrowsIfNoCtorAvailable()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C5>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C5>());
        }

        [Test]
        public void DoesNotChooseCopyConstructor()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C11>().ToSelf();
            var ioc = builder.BuildContainer();

            // This actually causes a StackOverflow on failure...
            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C11>());
        }

        [Test]
        public void SingletonActuallySingleton()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            builder.Bind<C2>().ToSelf();
            builder.Bind<C3>().ToSelf();
            var ioc = builder.BuildContainer();

            var c3 = ioc.Get<C3>();
            Assert.AreEqual(ioc.Get<C1>(), c3.C1);
            Assert.AreEqual(ioc.Get<C2>().C1, c3.C1);
        }

        [Test]
        public void IEnumerableHasAllInjected()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().To<C1>();
            builder.Bind<I1>().To<C1>();
            builder.Bind<I1>().To<C2>();
            builder.Bind<C8>().ToSelf();
            var ioc = builder.BuildContainer();

            var c8 = ioc.Get<C8>();
            var i1s = c8.I1s.ToList();

            Assert.AreEqual(2, i1s.Count);
            Assert.IsInstanceOf<C1>(i1s[0]);
            Assert.IsInstanceOf<C2>(i1s[1]);
        }

        [Test]
        public void ThrowsIfCantResolveAttributedConstructor()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C6>().ToSelf();
            var ioc = builder.BuildContainer();
            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C6>());
        }

        [Test]
        public void ThrowsIfResolvingParamFailsForSomeReason()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C1>();
            builder.Bind<I1>().To<C2>();
            builder.Bind<C9>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCRegistrationException>(() => ioc.Get<C9>());
        }

        [Test]
        public void ThrowsIfCollectionTypeCantBeResolved()
        {
            // This test is needed to hit a condition in TryRetrieveGetAllRegistrationFromElementType
            // where a collection type is constructed, but is unsuitable

            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C10>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C10>());
        }
    }
}
