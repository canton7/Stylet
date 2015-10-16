using NUnit.Framework;
using StyletIoC;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCGetSingleTests
    {
        interface IC1 { }
        class C1 : IC1 { }
        class C12 : IC1 { }
        class C2
        {
            public C1 C1;
            public C2(C1 c1)
            {
                this.C1 = c1;
            }
        }
        public class C3 : IDisposable
        {
            public bool Disposed;
            public void Dispose()
            {
                this.Disposed = true;
            }
        }

        [Test]
        public void SelfTransientBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SelfTransientBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C1)).ToSelf();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void SelfSingletonBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void SelfSingletonBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C1)).ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void FactoryTransientBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToFactory(c => new C1());
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void FactoryTransientBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C1)).ToFactory(c => new C1());
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void FactorySingletonBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToFactory(c => new C1()).InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<C1>();
            var obj2 = ioc.Get<C1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void FactorySingletonBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C1)).ToFactory(c => new C1()).InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(C1));
            var obj2 = ioc.Get(typeof(C1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ImplementationTransientBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C1>();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<IC1>();
            var obj2 = ioc.Get<IC1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ImplementationTransientBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(IC1)).To(typeof(C1));
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(IC1));
            var obj2 = ioc.Get(typeof(IC1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ImplementationSingletonBindingResolvesGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C1>().InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get<IC1>();
            var obj2 = ioc.Get<IC1>();

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ImplementationSingletonBindingResolvesTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(IC1)).To(typeof(C1)).InSingletonScope();
            var ioc = builder.BuildContainer();

            var obj1 = ioc.Get(typeof(IC1));
            var obj2 = ioc.Get(typeof(IC1));

            Assert.That(obj1, Is.Not.Null);
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void DisposedSingletonRegistrationDoesNotRetainInstance()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var weakRef = new WeakReference(ioc.Get<C1>());
            ioc.Dispose();
            GC.Collect();
            Assert.IsFalse(weakRef.IsAlive);
        }

        [Test]
        public void ThrowsIfMoreThanOneRegistrationFound()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C1>();
            builder.Bind<IC1>().To<C12>();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCRegistrationException>(() => ioc.Get<IC1>());
        }

        [Test]
        public void ThrowsIfSameBindingAppearsMultipleTimes()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C1>();
            builder.Bind<IC1>().To<C1>();
            Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
        }

        [Test]
        public void DoesNotThrowIfSameBindingAppearsMultipleTimesWithDifferentKeys()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C1>();
            builder.Bind<IC1>().To<C1>().WithKey("key");
            Assert.DoesNotThrow(() => builder.BuildContainer());
        }

        [Test]
        public void ThrowsIfTypeIsNull()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();
            Assert.Throws<ArgumentNullException>(() => ioc.Get(null));
        }

        [Test]
        public void CachedFactoryInstanceExpressionWorks()
        {
            // The factory's instance expression can be cached. This ensures that that works
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToFactory(x => new C1());
            builder.Bind<C2>().ToSelf();
            var ioc = builder.BuildContainer();

            var c1 = ioc.Get<C1>();
            var c2 = ioc.Get<C2>();

            Assert.NotNull(c2.C1);
            Assert.AreNotEqual(c1, c2.C1);
        }

        [Test]
        public void SingletonRegistrationDisposesDisposableInstanceWhenParentDisposed()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C3>().ToSelf().InSingletonScope();
            var ioc = builder.BuildContainer();

            var c3 = ioc.Get<C3>();
            ioc.Dispose();
            Assert.IsTrue(c3.Disposed);
        }

        [Test]
        public void ContainerThrowsIfDisposedThenMethodCalled()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();

            ioc.Dispose();

            Assert.Throws<ObjectDisposedException>(() => ioc.Get<C1>());
            Assert.Throws<ObjectDisposedException>(() => ioc.Get(typeof(C1)));
            Assert.Throws<ObjectDisposedException>(() => ioc.GetTypeOrAll<C1>());
            Assert.Throws<ObjectDisposedException>(() => ioc.GetTypeOrAll(typeof(C1)));
        }

        [Test]
        public void IContainerIsAvailable()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();

            Assert.AreEqual(ioc, ioc.Get<IContainer>());
        }
    }
}
