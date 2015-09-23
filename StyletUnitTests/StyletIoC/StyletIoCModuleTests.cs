using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests.StyletIoC
{
    [TestFixture]
    public class StyletIoCModuleTests
    {
        class C1 { }
        class C2 { }

        class ModuleA : StyletIoCModule
        {
            protected override void Load()
            {
                Bind<C1>().ToSelf();
            }
        }

        class ModuleB : StyletIoCModule
        {
            protected override void Load()
            {
                Bind<C2>().ToSelf();
            }
        }

        [Test]
        public void BuilderAddsBindingsFromModule()
        {
            var builder = new StyletIoCBuilder(new ModuleA());
            var ioc = builder.BuildContainer();

            Assert.IsInstanceOf<C1>(ioc.Get<C1>());
        }

        [Test]
        public void BuilderAddsBindingsFromModuleAddedWithAddModule()
        {
            var builder = new StyletIoCBuilder();
            builder.AddModule(new ModuleA());
            var ioc = builder.BuildContainer();

            Assert.IsInstanceOf<C1>(ioc.Get<C1>());
        }

        [Test]
        public void BuilderAddsBindingsFromModulesAddedWithAddModules()
        {
            var builder = new StyletIoCBuilder();
            builder.AddModules(new ModuleA(), new ModuleB());
            var ioc = builder.BuildContainer();

            Assert.IsInstanceOf<C1>(ioc.Get<C1>());
            Assert.IsInstanceOf<C2>(ioc.Get<C2>());
        }
    }
}
