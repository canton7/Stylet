using NUnit.Framework;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests.StyletIoC
{
    [TestFixture]
    public class StyletIoCChildContainerTests
    {
        class C1 { }
        class C2
        {
            public C1 C1;
            public C2(C1 c1) { this.C1 = c1; }
        }
        class C3
        {
            public bool C1CtorCalled;
            public bool NoArgsCtorCalled;

            public C3(C1 c2)
            {
                this.C1CtorCalled = true;
            }
            public C3()
            {
                this.NoArgsCtorCalled = true;
            }
        }
        class C4<T> { }
        interface IValidator<T> { }
        class StringValidator : IValidator<string> { }
        class IntValidator : IValidator<int> { }
        class C5 : IDisposable
        {
            public bool Disposed;
            public void Dispose() { this.Disposed = true; }
        }

        [Test]
        public void ChildContainerCanAccessRegistrationsOnParent()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<C2>().ToSelf();
            var parent = builder.BuildContainer();
            var child = parent.CreateChildBuilder().BuildContainer();

            var c2 = child.Get<C2>();
            Assert.NotNull(c2);
            Assert.NotNull(c2.C1);
        }

        [Test]
        public void ChildContainerSharesParentSingletons()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            var parent = builder.BuildContainer();
            var child = parent.CreateChildBuilder().BuildContainer();

            Assert.AreEqual(parent.Get<C1>(), child.Get<C1>());
        }

        [Test]
        public void ChildContainersDontShareSingletons()
        {
            var builder = new StyletIoCBuilder();
            var parent = builder.BuildContainer();
            var child1Builder = parent.CreateChildBuilder();
            child1Builder.Bind<C1>().ToSelf().InSingletonScope();
            var child1 = child1Builder.BuildContainer();
            var child2Builder = parent.CreateChildBuilder();
            child2Builder.Bind<C1>().ToSelf().InSingletonScope();
            var child2 = child2Builder.BuildContainer();

            Assert.AreNotEqual(child1.Get<C1>(), child2.Get<C1>());
        }

        [Test]
        public void ResolvingOnParentDoesNotTakeChildIntoAccount_CalledFromChild()
        {
            // It will have to pick the C3 ctor which doesn't require C1, as C1 exists only in the child
            var builder = new StyletIoCBuilder();
            builder.Bind<C3>().ToSelf();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<C1>().ToSelf();
            var child = childBuilder.BuildContainer();

            // Compile it in the context of the child, first
            var c3 = child.Get<C3>();
            Assert.IsTrue(c3.NoArgsCtorCalled);
        }

        [Test]
        public void ResolvingOnParentDoesNotTakeChildIntoAccount_CalledFromParent()
        {
            // It will have to pick the C3 ctor which doesn't require C1, as C1 exists only in the child
            var builder = new StyletIoCBuilder();
            builder.Bind<C3>().ToSelf();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<C1>().ToSelf();
            var child = childBuilder.BuildContainer();

            // Now compile from the parent
            var c3 = parent.Get<C3>();
            Assert.IsTrue(c3.NoArgsCtorCalled);
        }

        [Test]
        public void RegistrationsOnChildDoNotAffectParent()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<C2>().ToSelf();
            var child = childBuilder.BuildContainer();

            Assert.Throws<StyletIoCRegistrationException>(() => parent.Get<C2>());
        }

        [Test]
        public void RecreatingSingletonBindingIntroducedNewScope()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InSingletonScope();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<C1>().ToSelf().InSingletonScope();
            var child = childBuilder.BuildContainer();

            Assert.AreNotEqual(parent.Get<C1>(), child.Get<C1>());
            Assert.AreEqual(parent.Get<C1>(), parent.Get<C1>());
            Assert.AreEqual(child.Get<C1>(), child.Get<C1>());
        }

        [Test]
        public void FactoriesResolveUsingTheResolutionContextOnWhichTheyWereRequested()
        {
            IRegistrationContext factoryContext = null;

            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToFactory(c =>
            {
                factoryContext = c;
                return new C1();
            });
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            parent.Get<C1>();
            Assert.AreEqual(parent, factoryContext);

            child.Get<C1>();
            Assert.AreEqual(child, factoryContext);
        }

        [Test]
        public void ChildInheritsParentsUnboundGenerics()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(C4<>)).ToSelf();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            Assert.IsInstanceOf<C4<int>>(child.Get<C4<int>>());
        }

        [Test]
        public void ChildInheritsParentsBoundGenerics()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IValidator<string>>().To<StringValidator>();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<IValidator<int>>().To<IntValidator>();
            var child = childBuilder.BuildContainer();

            Assert.DoesNotThrow(() => child.Get<IValidator<string>>());

            Assert.Throws<StyletIoCRegistrationException>(() => parent.Get<IValidator<int>>());
            Assert.DoesNotThrow(() => child.Get<IValidator<int>>());
            Assert.Throws<StyletIoCRegistrationException>(() => parent.Get<IValidator<int>>());
        }

        [Test]
        public void ContainerDisposesItsSingletonsWhenRequested()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C5>().ToSelf().InSingletonScope();
            var parent = builder.BuildContainer();

            var c5 = parent.Get<C5>();
            Assert.False(c5.Disposed);

            parent.Dispose();
            Assert.True(c5.Disposed);
            Assert.Throws<ObjectDisposedException>(() => parent.Get<C5>());
        }

        [Test]
        public void DisposingContainerDoesNotDisposeParentSingletons()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C5>().ToSelf().InSingletonScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            // Make sure it actually gets built
            var c5 = parent.Get<C5>();

            child.Dispose();

            Assert.False(parent.Get<C5>().Disposed);
        }
    }
}
