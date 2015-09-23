using NUnit.Framework;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCGetAllTests
    {
        interface IC1 { }
        class C11 : IC1 { }
        class C12 : IC1 { }
        class C13 : IC1 { }

        interface IC2 { }
        class C21 : IC2 { }
        class C22 : IC2 { }

        class C3
        {
            public List<IC2> C2s;
            public C3(IEnumerable<IC2> c2s)
            {
                this.C2s = c2s.ToList();
            }
        }

        // Tests that Bind() and friends worked was done in StyletIoCGetSingleTests

        [Test]
        public void ImplementationTransientBindingsResolveGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C11>();
            builder.Bind<IC1>().To<C12>();
            builder.Bind<IC1>().To<C13>();
            builder.Bind<IC2>().To<C21>();
            var ioc = builder.BuildContainer();

            var results1 = ioc.GetAll<IC1>().ToList();
            var results2 = ioc.GetAll<IC1>().ToList();

            Assert.AreEqual(3, results1.Count);

            Assert.IsInstanceOf<C11>(results1[0]);
            Assert.IsInstanceOf<C12>(results1[1]);
            Assert.IsInstanceOf<C13>(results1[2]);

            Assert.That(results1, Is.Not.EquivalentTo(results2));
        }

        [Test]
        public void ImplementationTransientBindingsResolveTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(IC1)).To(typeof(C11));
            builder.Bind(typeof(IC1)).To(typeof(C12));
            builder.Bind(typeof(IC1)).To(typeof(C13));
            builder.Bind(typeof(IC2)).To(typeof(C21));
            var ioc = builder.BuildContainer();

            var results1 = ioc.GetAll(typeof(IC1)).ToList();
            var results2 = ioc.GetAll(typeof(IC1)).ToList();

            Assert.AreEqual(3, results1.Count);
            Assert.IsInstanceOf<C11>(results1[0]);
            Assert.IsInstanceOf<C12>(results1[1]);
            Assert.IsInstanceOf<C13>(results1[2]);

            Assert.That(results1, Is.Not.EquivalentTo(results2));
        }

        [Test]
        public void SingletonBindingsResolveGeneric()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C11>().InSingletonScope();
            builder.Bind<IC1>().To<C12>().InSingletonScope();
            builder.Bind<IC1>().To<C13>().InSingletonScope();
            builder.Bind<IC2>().To<C21>().InSingletonScope();
            var ioc = builder.BuildContainer();

            var results1 = ioc.GetAll<IC1>().ToList();
            var results2 = ioc.GetAll<IC1>().ToList();

            Assert.AreEqual(3, results1.Count);
            Assert.IsInstanceOf<C11>(results1[0]);
            Assert.IsInstanceOf<C12>(results1[1]);
            Assert.IsInstanceOf<C13>(results1[2]);
                
            Assert.That(results1, Is.EquivalentTo(results2));
        }

        [Test]
        public void SingletonBindingsResolveTyped()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind(typeof(IC1)).To(typeof(C11)).InSingletonScope();
            builder.Bind(typeof(IC1)).To(typeof(C12)).InSingletonScope();
            builder.Bind(typeof(IC1)).To(typeof(C13)).InSingletonScope();
            builder.Bind(typeof(IC2)).To(typeof(C21)).InSingletonScope();
            var ioc = builder.BuildContainer(); 

            var results1 = ioc.GetAll(typeof(IC1)).ToList();
            var results2 = ioc.GetAll(typeof(IC1)).ToList();

            Assert.AreEqual(3, results1.Count);
            Assert.IsInstanceOf<C11>(results1[0]);
            Assert.IsInstanceOf<C12>(results1[1]);
            Assert.IsInstanceOf<C13>(results1[2]);

            Assert.That(results1, Is.EquivalentTo(results2));
        }

        [Test]
        public void GetAllReturnsSingleInstanceIfOnlyOneRegistration()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C11>();
            var ioc = builder.BuildContainer();

            var results = ioc.GetAll<IC1>().ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsInstanceOf<C11>(results[0]);
        }

        [Test]
        public void GetAllDoesNotThrowIfNoRegistrationsFound()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();
            Assert.DoesNotThrow(() => ioc.GetAll<IC1>());
        }

        [Test]
        public void GetAllThrowsIfTypeIsNull()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();
            Assert.Throws<ArgumentNullException>(() => ioc.GetAll(null));
        }

        // Also cover GetTypeOrAll here
        [Test]
        public void GetTypeOrAllReturnsSingleIfOneRegistration()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C11>();
            var ioc = builder.BuildContainer();

            var result = ioc.GetTypeOrAll<IC1>();
            Assert.IsInstanceOf<C11>(result);
        }

        [Test]
        public void GetTypeOrAllReturnsCollectionIfManyRegistrations()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<IC1>().To<C11>();
            builder.Bind<IC1>().To<C12>();
            var ioc = builder.BuildContainer();

            var result = ioc.GetTypeOrAll<IEnumerable<IC1>>();
            Assert.IsInstanceOf<IEnumerable<IC1>>(result);

            var list = ((IEnumerable<IC1>)result).ToList();
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOf<C11>(list[0]);
            Assert.IsInstanceOf<C12>(list[1]);
        }

        [Test]
        public void GetTypeOrAllThrowsIfTypeIsNull()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();
            Assert.Throws<ArgumentNullException>(() => ioc.GetTypeOrAll(null));
        }

        [Test]
        public void CachedGetAllExpressionWorks()
        {
            // The GetAll creator's instance expression can be cached. This ensures that that works
            var builder = new StyletIoCBuilder();
            builder.Bind<IC2>().To<C21>();
            builder.Bind<IC2>().To<C22>();
            builder.Bind<C3>().ToSelf();
            var ioc = builder.BuildContainer();

            var c2s = ioc.GetAll<IC2>().ToList();
            var c3 = ioc.Get<C3>();

            Assert.NotNull(c3.C2s);
            Assert.AreEqual(2, c3.C2s.Count);
            Assert.AreNotEqual(c2s[0], c3.C2s[0]);
            Assert.AreNotEqual(c2s[1], c3.C2s[1]);
        }
    }
}
