using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests.StyletIoC;

[TestFixture]
public class StyletIoCPropertyInjectionTests
{
    private class C1 { }

    private interface I2 { }

    private class C2 : I2 { }

    private class Subject1
    {
        public C1 Ignored = null;

        [Inject]
        public C1 C1 = null;
    }

    private class Subject2
    {
        [Inject]
#pragma warning disable IDE0044 // Add readonly modifier
        private C1 c1 = null;
#pragma warning restore IDE0044 // Add readonly modifier
        public C1 GetC1() { return this.c1; }
    }

    private class Subject3
    {
        [Inject]
        public C1 C1 { get; set; }
    }

    private class Subject4
    {
        [Inject]
        public C1 C11 { get; private set; }
        [Inject]
        private C1 c12 { get; set; }

        public C1 GetC11() => this.C11;
        public C1 GetC12() => this.c12;
    }

    private class Subject5
    {
        [Inject("key")]
        public C1 C1 = null;
    }

    private class Subject6 : IInjectionAware
    {
        [Inject]
        public C1 C1 = null;

        public bool ParametersInjectedCalledCorrectly;
        public void ParametersInjected() { this.ParametersInjectedCalledCorrectly = this.C1 != null; }
    }

    private class C3
    {
        public C3(C4 c2) { }
    }

    private class C4
    {
        public C4(C3 c3) { }
    }

    [Test]
    public void BuildsUpPublicFields()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject1();
        ioc.BuildUp(subject);

        Assert.IsInstanceOf<C1>(subject.C1);
        Assert.IsNull(subject.Ignored);
    }

    [Test]
    public void BuildsUpPrivateFields()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject2();
        ioc.BuildUp(subject);

        Assert.IsInstanceOf<C1>(subject.GetC1());
    }

    [Test]
    public void BuildsUpPublicProperties()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject3();
        ioc.BuildUp(subject);

        Assert.IsInstanceOf<C1>(subject.C1); 
    }

    [Test]
    public void BuildsUpPrivateProperties()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject4();
        ioc.BuildUp(subject);

        Assert.IsInstanceOf<C1>(subject.GetC11());
        Assert.IsInstanceOf<C1>(subject.GetC12());
    }

    [Test]
    public void RespectsKeys()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf().WithKey("key");
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject5();
        ioc.BuildUp(subject);

        Assert.IsInstanceOf<C1>(subject.C1);
    }

    [Test]
    public void ThrowsIfCanNotResolve()
    {
        var builder = new StyletIoCBuilder();
        IContainer ioc = builder.BuildContainer();

        var subject = new Subject1();
        Assert.Throws<StyletIoCRegistrationException>(() => ioc.BuildUp(subject));
    }

    [Test]
    public void BuildsUpParametersOfNewlyCreatedType()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        builder.Bind<Subject1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        Subject1 subject = ioc.Get<Subject1>();

        Assert.IsInstanceOf<C1>(subject.C1);
        Assert.IsNull(subject.Ignored);
    }

    [Test]
    public void CallsParametersInjectedAfterInjectingParameters()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        builder.Bind<Subject6>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        Subject6 subject = ioc.Get<Subject6>();

        Assert.IsInstanceOf<C1>(subject.C1);
        Assert.IsTrue(subject.ParametersInjectedCalledCorrectly);
    }

    [Test]
    public void FactoryCreatorBuildsUp()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        builder.Bind<Subject1>().ToFactory(c => new Subject1());
        IContainer ioc = builder.BuildContainer();

        Subject1 subject = ioc.Get<Subject1>();

        Assert.IsInstanceOf<C1>(subject.C1);
    }

    [Test]
    public void BuildUpDoesNotReplaceAlreadySetProperties()
    {
        var builder = new StyletIoCBuilder();
        builder.Bind<C1>().ToSelf();
        IContainer ioc = builder.BuildContainer();

        var s = new Subject1();
        ioc.BuildUp(s);
        C1 firstC1 = s.C1;
        ioc.BuildUp(s);
        Assert.AreEqual(s.C1, firstC1);
    }
}
