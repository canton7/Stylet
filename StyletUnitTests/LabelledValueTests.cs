using NUnit.Framework;
using Stylet;
using System.Collections.Generic;

namespace StyletUnitTests
{
    [TestFixture]
    public class LabelledValueTests
    {
        [Test]
        public void StoresLabelAndValue()
        {
            var lbv = new LabelledValue<int>("an int", 5);
            Assert.AreEqual("an int", lbv.Label);
            Assert.AreEqual(5, lbv.Value);
        }

        [Test]
        public void SettingLabelAndValueRaisePropertyChangedNotifications()
        {
            var lbv = new LabelledValue<float>();

            lbv.PropertyChangedDispatcher = a => a();
            var changedProperties = new List<string>();
            lbv.PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);

            lbv.Label = "hello";
            lbv.Value = 2.2f;

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Label", "Value" }));
        }

        [Test]
        public void EqualsIdenticalLabelledValues()
        {
            var lbv1 = new LabelledValue<int>("int", 5);
            var lbv2 = new LabelledValue<int>("int", 5);

            // Test both variants
            Assert.That(lbv1.Equals(lbv2));
            Assert.That(lbv1.Equals((object)lbv2));
        }

        [Test]
        public void NotEqualLabeledValuesOfDifferentTypes()
        {
            var lbv1 = new LabelledValue<int>("test", 5);
            var lbv2 = new LabelledValue<double>("test", 5.0);

            Assert.False(lbv1.Equals(lbv2));
        }

        [Test]
        public void NotEqualNull()
        {
            var lbv = new LabelledValue<int>("test", 5);
            Assert.False(lbv.Equals(null));
        }

        [Test]
        public void NotEqualDifferentLabel()
        {
            var lbv1 = new LabelledValue<int>("test", 5);
            var lbv2 = new LabelledValue<int>("testy", 5);
            Assert.IsFalse(lbv1.Equals(lbv2));
        }

        [Test]
        public void NotEqualDifferentValues()
        {
            var lbv1 = new LabelledValue<int>("test", 5);
            var lbv2 = new LabelledValue<int>("test", 6);
            Assert.IsFalse(lbv1.Equals(lbv2));
        }

        [Test]
        public void GetHashCodeSameForIdenticalLabelledValues()
        {
            var lbv1 = new LabelledValue<int>("int", 5);
            var lbv2 = new LabelledValue<int>("int", 5);

            Assert.AreEqual(lbv1.GetHashCode(), lbv2.GetHashCode());
        }

        [Test]
        public void ToStringReturnsLabel()
        {
            var lbv = new LabelledValue<int>("label", 3);
            Assert.AreEqual("label", lbv.ToString());
        }

        [Test]
        public void CreateCreatesLabelledValue()
        {
            var lbv = LabelledValue.Create("test", 2.2f);
            Assert.AreEqual(new LabelledValue<float>("test", 2.2f), lbv);
        }
    }
}
