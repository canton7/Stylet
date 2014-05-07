using NUnit.Framework;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class BoolToVisibilityConverterTests
    {
        private BoolToVisibilityConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new BoolToVisibilityConverter();
        }

        [Test]
        public void InstanceReturnsSingleton()
        {
            Assert.IsNotNull(BoolToVisibilityConverter.Instance);
            Assert.AreEqual(BoolToVisibilityConverter.Instance, BoolToVisibilityConverter.Instance);
        }

        [Test]
        public void ConvertReturnsNullIfValueNotBool()
        {
            var result = this.converter.Convert(new object(), null, null, null);
            Assert.Null(result);
        }

        [Test]
        public void ConvertReturnsVisibleForTrueByDefault()
        {
            var result = this.converter.Convert(true, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void ConvertReturnsSetTrueVisibility()
        {
            this.converter.TrueVisibility = Visibility.Hidden;
            var result = this.converter.Convert(true, null, null, null);
            Assert.AreEqual(Visibility.Hidden, result);
        }

        [Test]
        public void ConvertReturnsCollapsedForFalseByDefault()
        {
            var result = this.converter.Convert(false, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void ConvertReturnsSetFalseVisibility()
        {
            this.converter.FalseVisibility = Visibility.Visible;
            var result = this.converter.Convert(false, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void ConvertBackReturnsTrueIfValueIsTrueVisibility()
        {
            this.converter.TrueVisibility = Visibility.Hidden;
            var result = this.converter.ConvertBack(Visibility.Hidden, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void ConvertBackReturnsFalseIfValueIsFalseVisibility()
        {
            this.converter.FalseVisibility = Visibility.Hidden;
            var result = this.converter.ConvertBack(Visibility.Hidden, null, null, null);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ConvertBackReturnsNullIfValueIsNotAVisibility()
        {
            var result = this.converter.ConvertBack(new object(), null, null, null);
            Assert.Null(result);
        }

        [Test]
        public void ConvertBackReturnsNullIfValueIsNotAConfiguredVisibility()
        {
            var result = this.converter.ConvertBack(Visibility.Hidden, null, null, null);
            Assert.Null(result);
        }
    }
}
