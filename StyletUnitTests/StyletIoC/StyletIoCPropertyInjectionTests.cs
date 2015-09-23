using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCPropertyInjectionTests
    {
        class C1 { }
        interface I2 { }
        class C2 : I2 { }

        class Subject1
        {
            public C1 Ignored = null;

            [Inject]
            public C1 C1 = null;
        }

        class Subject2
        {
            [Inject]
            private C1 c1 = null;
            public C1 GetC1() { return this.c1; }
        }

        class Subject3
        {
            [Inject]
            public C1 C1 { get; set; }
        }

        class Subject4
        {
            [Inject]
            public C1 C11 { get; private set; }
            [Inject]
            private C1 C12 { get; set; }

            public C1 GetC11() { return this.C11; }
            public C1 GetC12() { return this.C12; }
        }

        class Subject5
        {
            [Inject("key")]
            public C1 C1 = null;
        }

        class Subject6 : IInjectionAware
        {
            [Inject]
            public C1 C1 = null;

            public bool ParametersInjectedCalledCorrectly;
            public void ParametersInjected() { this.ParametersInjectedCalledCorrectly = this.C1 != null; }
        }

        class C3
        {
            public C3(C4 c2) { }
        }
        class C4
        {
            public C4(C3 c3) { }
        }

        [Test]
        public void BuildsUpPublicFields()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

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
            var ioc = builder.BuildContainer();

            var subject = new Subject2();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.GetC1());
        }

        [Test]
        public void BuildsUpPublicProperties()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            var subject = new Subject3();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.C1); 
        }

        [Test]
        public void BuildsUpPrivateProperties()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

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
            var ioc = builder.BuildContainer();

            var subject = new Subject5();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.C1);
        }

        [Test]
        public void ThrowsIfCanNotResolve()
        {
            var builder = new StyletIoCBuilder();
            var ioc = builder.BuildContainer();

            var subject = new Subject1();
            Assert.Throws<StyletIoCRegistrationException>(() => ioc.BuildUp(subject));
        }

        [Test]
        public void BuildsUpParametersOfNewlyCreatedType()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<Subject1>().ToSelf();
            var ioc = builder.BuildContainer();

            var subject = ioc.Get<Subject1>();

            Assert.IsInstanceOf<C1>(subject.C1);
            Assert.IsNull(subject.Ignored);
        }

        [Test]
        public void CallsParametersInjectedAfterInjectingParameters()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<Subject6>().ToSelf();
            var ioc = builder.BuildContainer();

            var subject = ioc.Get<Subject6>();

            Assert.IsInstanceOf<C1>(subject.C1);
            Assert.IsTrue(subject.ParametersInjectedCalledCorrectly);
        }

        [Test]
        public void FactoryCreatorBuildsUp()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            builder.Bind<Subject1>().ToFactory(c => new Subject1());
            var ioc = builder.BuildContainer();

            var subject = ioc.Get<Subject1>();

            Assert.IsInstanceOf<C1>(subject.C1);
        }

        [Test]
        public void BuildUpDoesNotReplaceAlreadySetProperties()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();

            var s = new Subject1();
            ioc.BuildUp(s);
            var firstC1 = s.C1;
            ioc.BuildUp(s);
            Assert.AreEqual(s.C1, firstC1);
        }
    }
}
