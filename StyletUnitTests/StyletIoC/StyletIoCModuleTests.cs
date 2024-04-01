using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests.StyletIoC;

[TestFixture]
public class StyletIoCModuleTests
{
    private class C1 { }

    private class C2 { }

    private class ModuleA : StyletIoCModule
    {
        protected override void Load()
        {
            this.Bind<C1>().ToSelf();
        }
    }

    private class ModuleB : StyletIoCModule
    {
        protected override void Load()
        {
            this.Bind<C2>().ToSelf();
        }
    }

    [Test]
    public void BuilderAddsBindingsFromModule()
    {
        var builder = new StyletIoCBuilder(new ModuleA());
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C1>(ioc.Get<C1>());
    }

    [Test]
    public void BuilderAddsBindingsFromModuleAddedWithAddModule()
    {
        var builder = new StyletIoCBuilder();
        builder.AddModule(new ModuleA());
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C1>(ioc.Get<C1>());
    }

    [Test]
    public void BuilderAddsBindingsFromModulesAddedWithAddModules()
    {
        var builder = new StyletIoCBuilder();
        builder.AddModules(new ModuleA(), new ModuleB());
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C1>(ioc.Get<C1>());
        Assert.IsInstanceOf<C2>(ioc.Get<C2>());
    }
}
