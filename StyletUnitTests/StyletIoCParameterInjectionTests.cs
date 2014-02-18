using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCParameterInjectionTests
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

        class Subject6
        {
            [Inject]
            public C1 C1 = null;

            public bool ParametersInjectedCalledCorrectly;
            public void ParametersInjected() { this.ParametersInjectedCalledCorrectly = this.C1 != null; }
        }

        [Test]
        public void BuildsUpPublicFields()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            var subject = new Subject1();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.C1);
            Assert.IsNull(subject.Ignored);
        }

        [Test]
        public void BuildsUpPrivateFields()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            var subject = new Subject2();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.GetC1());
        }

        [Test]
        public void BuildsUpPublicProperties()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            var subject = new Subject3();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.C1); 
        }

        [Test]
        public void BuildsUpPrivateProperties()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            var subject = new Subject4();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.GetC11());
            Assert.IsInstanceOf<C1>(subject.GetC12());
        }

        [Test]
        public void RespectsKeys()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf("key");
            var subject = new Subject5();
            ioc.BuildUp(subject);

            Assert.IsInstanceOf<C1>(subject.C1);
        }

        [Test]
        public void ThrowsIfCanNotResolve()
        {
            var ioc = new StyletIoC();
            var subject = new Subject1();
            Assert.Throws<StyletIoCRegistrationException>(() => ioc.BuildUp(subject));
        }

        [Test]
        public void BuildsUpParametersOfNewlyCreatedType()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<Subject1>().ToSelf();
            var subject = ioc.Get<Subject1>();

            Assert.IsInstanceOf<C1>(subject.C1);
            Assert.IsNull(subject.Ignored);
        }

        [Test]
        public void CallsParametersInjectedAfterInjectingParameters()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<Subject6>().ToSelf();
            var subject = ioc.Get<Subject6>();

            Assert.IsInstanceOf<C1>(subject.C1);
            Assert.IsTrue(subject.ParametersInjectedCalledCorrectly);
        }
    }
}
