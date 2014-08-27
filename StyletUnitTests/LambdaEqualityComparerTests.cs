using NUnit.Framework;
using Stylet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class LambdaEqualityComparerTests
    {
        [Test]
        public void CallsLambdaToCompareObjects()
        {
            int a = 0;
            int b = 0;
            var c = new LambdaEqualityComparer<int>((x, y) =>
            {
                a = x;
                b = y;
                return false;
            });

            var result = c.Equals(3, 4);

            Assert.AreEqual(3, a);
            Assert.AreEqual(4, b);
            Assert.IsFalse(result);
        }

        [Test]
        public void ThrowsIfNullLambdaPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new LambdaEqualityComparer<int>(null));
        }

        [Test]
        public void ReturnsHashCodeOfPassedObject()
        {
            var c = new LambdaEqualityComparer<int>((a, b) => false);
            Assert.AreEqual(5.GetHashCode(), c.GetHashCode(5));
        }

        [Test]
        public void NongenericCallsLambdaToCompareObjects()
        {
            int a = 0;
            int b = 0;
            var c = new LambdaEqualityComparer<int>((x, y) =>
            {
                a = x;
                b = y;
                return false;
            });

            var result = ((IEqualityComparer)c).Equals(3, 4);

            Assert.AreEqual(3, a);
            Assert.AreEqual(4, b);
            Assert.IsFalse(result);
        }

        [Test]
        public void NongenericReturnsHashCodeOfPassedObject()
        {
            var c = new LambdaEqualityComparer<int>((a, b) => false);
            Assert.AreEqual(5.GetHashCode(), ((IEqualityComparer)c).GetHashCode(5));
        }
    }
}
