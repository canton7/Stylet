using NUnit.Framework;
using Stylet;
using System;
using System.Linq.Expressions;

namespace StyletUnitTests
{
    [TestFixture]
    public class ExpressionExtensionsTests
    {
        private int property { get; set; }

        // Simulate how it's meant t be used
        private string NameForProperty<TProperty>(Expression<Func<TProperty>> property)
        {
            return property.NameForProperty();
        }

        [Test]
        public void NameForPropertyGetsNameIfMemberExpression()
        {
            Assert.AreEqual("property", this.NameForProperty(() => this.property));
        }
        
        [Test]
        public void NameForPropertyGetsNameIfUnaryExpression()
        {
            Expression<Func<object>> expression = () => this.property;
            Assert.AreEqual("property", expression.NameForProperty());
        }

        [Test]
        public void NameForPropertyThrowsIfNotMemberExpressionOrUnaryExpression()
        {
            Expression<Func<object>> expression = () => null;
            Assert.Throws<ArgumentException>(() => expression.NameForProperty());
        }
    }
}
