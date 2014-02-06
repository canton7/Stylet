using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class BindableCollectionTests
    {
        [Test]
        public void AddrangeFiresPropertyChanged()
        {
            var collection = new BindableCollection<Element>(new[] { new Element(), new Element() });

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);
 
            collection.AddRange(new[] { new Element(), new Element() });

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        private class Element { }
    }
}
