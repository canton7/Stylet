using NUnit.Framework;
using Stylet.Xaml;
using System;

namespace StyletUnitTests;

[TestFixture]
public class EqualityConverterTests
{
    private EqualityConverter converter;

    [SetUp]
    public void SetUp()
    {
        this.converter = new EqualityConverter();
    }

    [Test]
    public void InstanceReturnsASingletonInstance()
    {
        Assert.NotNull(EqualityConverter.Instance);
        Assert.AreEqual(EqualityConverter.Instance, EqualityConverter.Instance);
    }

    [Test]
    public void ReturnsNullIfNullPassed()
    {
        Assert.Null(this.converter.Convert(null, null, null, null));
    }

    [Test]
    public void ReturnsNullIfEmptyArrayPassed()
    {
        Assert.Null(this.converter.Convert(new object[0], null, null, null));
    }

    [Test]
    public void ReturnsTrueIfASingleItemGiven()
    {
        object value = this.converter.Convert(new object[] { 5 }, null, null, null);
        Assert.IsInstanceOf<bool>(value);
        Assert.True((bool)value);
    }

    [Test]
    public void ReturnsTrueIfAllItemsAreEqual()
    {
        object obj = new();
        object value = this.converter.Convert(new[] { obj, obj, obj }, null, null, null);
        Assert.IsInstanceOf<bool>(value);
        Assert.True((bool)value);
    }

    [Test]
    public void ReturnsFalseIfAllItemsAreEqualAndInvertIsTrue()
    {
        object obj = new();
        this.converter.Invert = true;
        object value = this.converter.Convert(new[] { obj, obj, obj }, null, null, null);
        Assert.IsInstanceOf<bool>(value);
        Assert.False((bool)value);
    }

    [Test]
    public void ReturnsFalseIfOneItemsDiffers()
    {
        object obj = new();
        object value = this.converter.Convert(new[] { obj, new object(), obj }, null, null, null);
        Assert.IsInstanceOf<bool>(value);
        Assert.False((bool)value);
    }

    [Test]
    public void ReturnsTrueIfOneItemsDiffersAndInvertIsTrue()
    {
        object obj = new();
        object value = this.converter.Convert(new[] { obj, new object(), obj }, null, null, null);
        Assert.IsInstanceOf<bool>(value);
        Assert.False((bool)value);
    }

    [Test]
    public void ConvertBackThrows()
    {
        Assert.Throws<NotImplementedException>(() => this.converter.ConvertBack(null, null, null, null));
    }
}
