using NUnit.Framework;
using Stylet.Xaml;
using System.Collections.Generic;

namespace StyletUnitTests
{
    using System.Globalization;

    [TestFixture]
    public class DebugConverterTests
    {
        private DebugConverter converter;
        private List<string> log;

        [SetUp]
        public void SetUp()
        {
            this.log = new List<string>();
            this.converter = new DebugConverter();
            this.converter.Logger = (msg, name) => log.Add(msg);
        }

        [Test]
        public void InstancePropertyIsSingleton()
        {
            Assert.AreEqual(DebugConverter.Instance, DebugConverter.Instance);
        }

        [Test]
        public void ConvertPassesThroughValue()
        {
            var result = this.converter.Convert(5, null, null, CultureInfo.InvariantCulture);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void ConvertBackPassesThrough()
        {
            var result = this.converter.ConvertBack("hello", null, null, CultureInfo.InvariantCulture);
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void NameIsUsedInLogger()
        {
            this.converter.Logger = (msg, name) => log.Add(name);
            this.converter.Name = "Test";
            this.converter.Convert(new object(), null, null, CultureInfo.InvariantCulture);

            Assert.That(this.log, Is.EquivalentTo(new[] { "Test" }));
        }

        [Test]
        public void LogsConvertWithNoParameter()
        {
            this.converter.Convert(5, typeof(int), null, CultureInfo.InvariantCulture);
            Assert.That(this.log, Is.EquivalentTo(new[] { "Convert: Value = '5' TargetType = 'System.Int32'" }));
        }

        [Test]
        public void LogsConvertWithParameter()
        {
            this.converter.Convert("hello", typeof(bool), "the parameter", CultureInfo.InvariantCulture);
            Assert.That(this.log, Is.EquivalentTo(new[] { "Convert: Value = 'hello' TargetType = 'System.Boolean' Parameter = 'the parameter'" }));
        }

        [Test]
        public void LogsConvertBackWithNoParameter()
        {
            this.converter.ConvertBack(2.2, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.That(this.log, Is.EquivalentTo(new[] { "ConvertBack: Value = '2.2' TargetType = 'System.String'" }));
        }

        [Test]
        public void LogsConvertBackWithParameter()
        {
            this.converter.ConvertBack(false, typeof(double), 5, CultureInfo.InvariantCulture);
            Assert.That(this.log, Is.EquivalentTo(new[] { "ConvertBack: Value = 'False' TargetType = 'System.Double' Parameter = '5'" }));
        }
    }
}
