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
    public class IoCTests
    {
        [Test]
        public void GetUsesGetInstance()
        {
            Type type = null;
            string key = null;
            IoC.GetInstance = (t, k) =>
            {
                type = t;
                key = k;
                return 5;
            };

            var result = IoC.Get<int>("hello");

            Assert.AreEqual(5, result);
            Assert.AreEqual(typeof(int), type);
            Assert.AreEqual("hello", key);
        }

        [Test]
        public void GetAllUsesGetAllInstances()
        {
            Type type = null;
            IoC.GetAllInstances = t =>
            {
                type = t;
                return new object[] { 1, 2, 3 };
            };

            var result = IoC.GetAll<int>();

            Assert.That(result, Is.EquivalentTo(new object[] { 1, 2, 3 }));
            Assert.AreEqual(typeof(int), type);
        }
    }
}
