using NUnit.Framework;
using StyletIoC;
using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
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
        interface I1 { }
        class C11 : I1 { }
        class C12 : I1 { }
        class C13 : I1 { }

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
        public void CreatingSameBindingOnParentAndChildCausesMultipleRegistrations_1()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C11>();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<I1>().To<C12>();
            var child = childBuilder.BuildContainer();

            var r = child.GetAll<I1>();

            Assert.AreEqual(1, parent.GetAll<I1>().Count());
            Assert.AreEqual(2, child.GetAll<I1>().Count());
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
        public void ChildExtendsButDoesNotModifyGetAllRegistrations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().To<C11>();
            builder.Bind<I1>().To<C12>();
            var parent = builder.BuildContainer();

            var childBuilder = parent.CreateChildBuilder();
            childBuilder.Bind<I1>().To<C13>();
            var child = childBuilder.BuildContainer();

            Assert.AreEqual(3, child.GetAll<I1>().Count());
            Assert.AreEqual(2, parent.GetAll<I1>().Count());
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

        [Test]
        public void DisposesPerChildRegistrationDoesNotRetainInstance()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InPerContainerScope();
            var ioc = builder.BuildContainer();

            var weakRef = new WeakReference(ioc.Get<C1>());
            ioc.Dispose();
            GC.Collect();
            Assert.IsFalse(weakRef.IsAlive);
        }

        [Test]
        public void ChildContainerScopeHasOneInstancePerScope()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InPerContainerScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            Assert.AreEqual(parent.Get<C1>(), parent.Get<C1>());
            Assert.AreEqual(child.Get<C1>(), child.Get<C1>());
            Assert.AreNotEqual(parent.Get<C1>(), child.Get<C1>());
        }

        [Test]
        public void KeyedChildContainerScopeHasOneInstancePerScope()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().WithKey("foo").InPerContainerScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            Assert.AreEqual(parent.Get<C1>("foo"), parent.Get<C1>("foo"));
            Assert.AreEqual(child.Get<C1>("foo"), child.Get<C1>("foo"));
            Assert.AreNotEqual(parent.Get<C1>("foo"), child.Get<C1>("foo"));
        }

        [Test]
        public void ChildContainerScopeDisposalDisposesCorrectThing()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C5>().ToSelf().InPerContainerScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            var parents = parent.Get<C5>();
            var childs = child.Get<C5>();

            child.Dispose();

            Assert.True(childs.Disposed);
            Assert.False(parents.Disposed);
        }

        [Test]
        public void UsingPerContainerRegistrationAfterDisposalPromptsException()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InPerContainerScope();
            var ioc = builder.BuildContainer();

            ioc.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ioc.Get<C1>());
        }

        [Test]
        public void FuncFactoryFetchesInstanceFromCorrectChild()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().InPerContainerScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            var funcFromParent = parent.Get<Func<C1>>();
            var funcFromChild = child.Get<Func<C1>>();

            Assert.AreEqual(parent.Get<C1>(), funcFromParent());
            Assert.AreEqual(child.Get<C1>(), funcFromChild());
        }

        [Test]
        public void FuncFactoryWithKeyFetchesInstanceFromCorrectChild()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf().WithKey("foo").InPerContainerScope();
            var parent = builder.BuildContainer();

            var child = parent.CreateChildBuilder().BuildContainer();

            var funcFromParent = parent.Get<Func<string, C1>>();
            var funcFromChild = child.Get<Func<string, C1>>();

            Assert.AreEqual(parent.Get<C1>("foo"), funcFromParent("foo"));
            Assert.AreEqual(child.Get<C1>("foo"), funcFromChild("foo"));
        }
    }
}
