using NUnit.Framework;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
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
            Assert.AreEqual(Visibility.Visible, BoolToVisibilityConverter.Instance.TrueVisibility);
            Assert.AreEqual(Visibility.Collapsed, BoolToVisibilityConverter.Instance.FalseVisibility);
        }

        [Test]
        public void InverseInstanceReturnsSingleton()
        {
            Assert.IsNotNull(BoolToVisibilityConverter.InverseInstance);
            Assert.AreEqual(BoolToVisibilityConverter.InverseInstance, BoolToVisibilityConverter.InverseInstance);
            Assert.AreEqual(Visibility.Collapsed, BoolToVisibilityConverter.InverseInstance.TrueVisibility);
            Assert.AreEqual(Visibility.Visible, BoolToVisibilityConverter.InverseInstance.FalseVisibility);
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
        public void ConvertTreatsZeroAsFalse()
        {
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0u, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0L, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0Lu, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0m, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0.0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(0.0f, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert((short)0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert((ushort)0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert((byte)0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert((sbyte)0, null, null, null));
        }

        [Test]
        public void ConvertTreatsNonZeroAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1u, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1L, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1Lu, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1m, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1.0, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(1.0f, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert((short)1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert((ushort)1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert((byte)1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert((sbyte)1, null, null, null));
        }

        [Test]
        public void ConvertTreatsNullAsFalse()
        {
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(null, null, null, null));
        }

        [Test]
        public void ConvertTreatsNonNullAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(new object(), null, null, null));
        }

        [Test]
        public void ConvertTreatsEmptyCollectionAsFalse()
        {
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(new int[0], null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(new List<object>(), null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert(new Dictionary<string, string>(), null, null, null));
        }

        [Test]
        public void ConvertTreatsNonEmptyCollectionAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(new int[1], null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(new List<object>() { 3 }, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(new Dictionary<string, string>() { { "A", "B" } }, null, null, null));
        }

        [Test]
        public void ConvertTreatsEmptyStringAsFalse()
        {
            Assert.AreEqual(Visibility.Collapsed, this.converter.Convert("", null, null, null));
        }

        [Test]
        public void ConvertTreatsNonEmptyStringAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert("hello", null, null, null));
        }

        [Test]
        public void ConvertTreatsRandomObjectAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(typeof(int), null, null, null));
        }

        [Test]
        public void ConvertTreatsRandomValueTypeAsTrue()
        {
            Assert.AreEqual(Visibility.Visible, this.converter.Convert(new KeyValuePair<int, int>(5, 5), null, null, null));
        }

        [Test]
        public void ConvertBackThrowsIfTargetTypeIsNotBool()
        {
            Assert.Throws<ArgumentException>(() => this.converter.ConvertBack(null, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsTrueIfValueIsTrueVisibility()
        {
            this.converter.TrueVisibility = Visibility.Hidden;
            var result = this.converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void ConvertBackReturnsFalseIfValueIsFalseVisibility()
        {
            this.converter.FalseVisibility = Visibility.Hidden;
            var result = this.converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ConvertBackReturnsNullIfValueIsNotAVisibility()
        {
            var result = this.converter.ConvertBack(new object(), typeof(bool), null, null);
            Assert.Null(result);
        }

        [Test]
        public void ConvertBackReturnsNullIfValueIsNotAConfiguredVisibility()
        {
            var result = this.converter.ConvertBack(Visibility.Hidden, typeof(bool), null, null);
            Assert.Null(result);
        }
    }
}
