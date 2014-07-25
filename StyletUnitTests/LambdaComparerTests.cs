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
    public class LambdaComparerTests
    {
        [Test]
        public void ThrowsIfNullLambdaPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new LambdaComparer<int>(null));
        }

        [Test]
        public void UsesLambdaToCompareObjects()
        {
            int a = 0;
            int b = 0;
            var c = new LambdaComparer<int>((x, y) =>
            {
                a = x;
                b = y;
                return -1;
            });

            var result = c.Compare(3, 4);

            Assert.AreEqual(3, a);
            Assert.AreEqual(4, b);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void NongenericThrowsIfXIsNotT()
        {
            var c = new LambdaComparer<int>((x, y) => -1);
            Assert.Throws<ArgumentException>(() => ((IComparer)c).Compare("cheese", 3));
        }

        [Test]
        public void NongenericThrowsIfYIsNotT()
        {
            var c = new LambdaComparer<int>((x, y) => -1);
            Assert.Throws<ArgumentException>(() => ((IComparer)c).Compare(3, "cheese"));
        }

        [Test]
        public void NongenericUsesLambdaToCompareObjects()
        {
            int a = 0;
            int b = 0;
            var c = new LambdaComparer<int>((x, y) =>
            {
                a = x;
                b = y;
                return -1;
            });

            var result = ((IComparer)c).Compare(3, 4);

            Assert.AreEqual(3, a);
            Assert.AreEqual(4, b);
            Assert.AreEqual(-1, result);
        }
    }
}
