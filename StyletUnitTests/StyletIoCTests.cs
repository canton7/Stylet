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
            ioc.BindSingleton<Dummy1, Dummy1>();
            //ioc.Bind<Dummy2, Dummy2>();
            ioc.BindFactory<Dummy2, Dummy2>(c => new Dummy2(c.Get<Dummy1>()));

            ioc.Compile();

            var two = ioc.Get<Dummy2>();
        }

        private class Dummy1
        {
        }

        private class Dummy2
        {
            public Dummy1 Dummy1;

            public Dummy2(Dummy1 dummy1)
            {
                this.Dummy1 = dummy1;
            }
        }
    }
}
