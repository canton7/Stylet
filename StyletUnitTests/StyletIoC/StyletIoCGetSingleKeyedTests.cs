using NUnit.Framework;
using StyletIoC;
using System.Linq;

namespace StyletUnitTests.StyletIoC;

[TestFixture]
public class StyletIoCGetSingleKeyedTests
{
    private interface IC { }

    private class C1 : IC { }

    private class C2 : IC { }

    private class C3 : IC { }

    [Inject("key1")]
    private class C4 : IC { }

    [Test]
    public void GetReturnsKeyedType()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<IC>().To<C1>().WithKey("key1");
        builder.Bind<IC>().To<C2>().WithKey("key2");
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C1>(ioc.Get<IC>("key1"));
        Assert.IsInstanceOf<C2>(ioc.Get<IC>("key2"));
    } 

    [Test]
    public void GetAllReturnsKeyedTypes()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<IC>().To<C1>().WithKey("key1");
        builder.Bind<IC>().To<C2>().WithKey("key1");
        builder.Bind<IC>().To<C3>();
        IContainer ioc = builder.BuildContainer();

        var results = ioc.GetAll<IC>("key1").ToList();

        Assert.AreEqual(results.Count, 2);
        Assert.IsInstanceOf<C1>(results[0]);
        Assert.IsInstanceOf<C2>(results[1]);
    }

    [Test]
    public void AttributeIsUsed()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<IC>().To<C3>();
        builder.Bind<IC>().To<C4>();
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C4>(ioc.Get<IC>("key1"));
    }

    [Test]
    public void GivenKeyOverridesAttribute()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<IC>().To<C3>();
        builder.Bind<IC>().To<C4>().WithKey("key2");
        IContainer ioc = builder.BuildContainer();

        Assert.IsInstanceOf<C4>(ioc.Get<IC>("key2"));
    }
}
