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
    public class StyletIoCTests
    {
        [Test]
        public void Temp()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<Dummy1>().ToSelf("test");
            //ioc.Bind<Dummy2, Dummy2>();
            ioc.Bind<Dummy2>().ToSelf();

            ioc.Compile();

            var two = ioc.Get(typeof(Dummy2), "test");
        }

        private class Dummy1
        {
        }

        [Inject("test")]
        private class Dummy2
        {
            public Dummy1 Dummy1;
            public string Foo;

            public Dummy2()
            {
            }

            [Inject]
            public Dummy2([Inject("test")] Dummy1 dummy1, string foo)
            {
                this.Dummy1 = dummy1;
                this.Foo = foo;
            }
        }
    }
}
