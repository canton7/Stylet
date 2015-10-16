using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests.StyletIoC
{
    [TestFixture]
    public class StyletIoCInstanceBindingTests
    {
        interface I1 { }
        class C1 : I1 { }

        [Test]
        public void InstanceBindingUsesInstanceToResolve()
        {
            var c1 = new C1();

            var builder = new StyletIoCBuilder();
            builder.Bind<I1>().ToInstance(c1);
            var ioc = builder.BuildContainer();

            Assert.AreEqual(c1, ioc.Get<I1>());
            Assert.AreEqual(c1, ioc.Get<I1>());
        }
    }
}
