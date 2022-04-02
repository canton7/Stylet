using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests.StyletIoC;

[TestFixture]
public class StyletIoCUnboundGenericTests
{
    private interface I1<T> { }

    private class C1<T> : I1<T> { }

    private class C12<T> { }

    private interface I2<T1, T2> { }

    private class C2<T1, T2> : I2<T2, T1> { }

    [Test]
    public void ResolvesSingleGenericType()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(C1<>)).ToSelf();
        IContainer ioc = builder.BuildContainer();

        Assert.DoesNotThrow(() => ioc.Get<C1<int>>());
    }

    [Test]
    public void ResolvesGenericTypeFromInterface()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(I1<>)).To(typeof(C1<>));
        IContainer ioc = builder.BuildContainer();

        I1<int> result = ioc.Get<I1<int>>();
        Assert.IsInstanceOf<C1<int>>(result);
    }

    [Test]
    public void ResolvesGenericTypeWhenOrderOfTypeParamsChanged()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(I2<,>)).To(typeof(C2<,>));
        IContainer ioc = builder.BuildContainer();

        I2<int, bool> c2 = ioc.Get<I2<int, bool>>();
        Assert.IsInstanceOf<C2<bool, int>>(c2);
    }

    [Test]
    public void ResolvesSingletonUnboundGeneric()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(I1<>)).To(typeof(C1<>)).InSingletonScope();
        IContainer ioc = builder.BuildContainer();

        Assert.AreEqual(ioc.Get<I1<int>>(), ioc.Get<I1<int>>());
    }

    [Test]
    public void ResolvesUnboundGenericFromKey()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(I1<>)).To(typeof(C1<>)).WithKey("test");
        IContainer ioc = builder.BuildContainer();

        Assert.NotNull(ioc.Get<I1<int>>("test"));
    }

    [Test]
    public void ThrowsIfMultipleRegistrationsForUnboundGeneric()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind(typeof(C1<>)).ToSelf();
        builder.Bind(typeof(C1<>)).ToSelf();
        Assert.Throws<StyletIoCRegistrationException>(() => builder.BuildContainer());
    }

    [Test]
    public void ThrowsIfUnboundGenericDoesNotImplementService()
    {
        var builder = new StyletIoCBuilder();
        Assert.Throws<StyletIoCRegistrationException>(() => builder.Bind(typeof(I1<>)).To(typeof(C12<>)));
    }
}
