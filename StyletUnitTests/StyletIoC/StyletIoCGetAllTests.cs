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
    public class StyletIoCGetAllTests
    {
        interface IC1 { }
        class C11 : IC1 { }
        class C12 : IC1 { }
        class C13 : IC1 { }

        interface IC2 { }
        class C21 : IC2 { }
        class C22 : IC2 { }

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

            Assert.AreEqual(results1.Count, 3);

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

            Assert.AreEqual(results1.Count, 3);
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

            Assert.AreEqual(results1.Count, 3);
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

            Assert.AreEqual(results1.Count, 3);
            Assert.IsInstanceOf<C11>(results1[0]);
            Assert.IsInstanceOf<C12>(results1[1]);
            Assert.IsInstanceOf<C13>(results1[2]);

            Assert.That(results1, Is.EquivalentTo(results2));
        }
    }
}
