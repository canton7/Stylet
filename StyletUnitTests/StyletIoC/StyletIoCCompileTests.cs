using NUnit.Framework;
using StyletIoC;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletIoCCompileTests
    {
        private class C1 { }
        private class C2
        {
            public C2(C1 c1) { }
        }

        [Test]
        public void CompileSucceedsIfNoErrors()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C1>().ToSelf();
            var ioc = builder.BuildContainer();
            
            Assert.DoesNotThrow(() => ioc.Compile());
            Assert.NotNull(ioc.Get<C1>());
        }

        [Test]
        public void CompileThrowsIfFindConstructorExceptionAndThrowOnErrorIsTrue()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C2>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Compile());
        }

        [Test]
        public void CompileDoesNotThrowIfFindConstructorExceptionAndThrowOnErrorIsFalse()
        {
            var builder = new StyletIoCBuilder();
            builder.Bind<C2>().ToSelf();
            var ioc = builder.BuildContainer();

            Assert.DoesNotThrow(() => ioc.Compile(false));
        }
    }
}
