using NUnit.Framework;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class IconToBitmapSourceConverterTests
    {
        private IconToBitmapSourceConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new IconToBitmapSourceConverter();
        }

        [Test]
        public void InstanceReturnsSingletonInstance()
        {
            Assert.IsInstanceOf<IconToBitmapSourceConverter>(IconToBitmapSourceConverter.Instance);
            Assert.AreEqual(IconToBitmapSourceConverter.Instance, IconToBitmapSourceConverter.Instance);
        }

        [Test]
        public void ReturnsNullIfValueIsNull()
        {
            var result = this.converter.Convert(null, null, null, null);
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnsNullIfNonObjectPassed()
        {
            var result = this.converter.Convert(5, null, null, null);
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertBackThrows()
        {
            Assert.Throws<NotImplementedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
