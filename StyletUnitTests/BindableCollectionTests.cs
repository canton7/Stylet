using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private class Element { }

        [Test]
        public void AddRangeAddsElements()
        {
            var itemsToAdd = new[] { new Element(), new Element() };
            var existingItems = new[] { new Element() };
            var collection = new BindableCollection<Element>(existingItems);

            collection.AddRange(itemsToAdd);

            Assert.AreEqual(existingItems.Concat(itemsToAdd), collection);
        }

        [Test]
        public void RemoveRangeRemovesElements()
        {
            var itemsToRemove = new[] { new Element(), new Element() };
            var existingItems = new[] { new Element() };
            var collection = new BindableCollection<Element>(itemsToRemove.Concat(existingItems));

            collection.RemoveRange(itemsToRemove);

            Assert.AreEqual(existingItems, collection);
        }

        [Test]
        public void AddRangeFiresPropertyChanged()
        {
            var collection = new BindableCollection<Element>(new[] { new Element(), new Element() });

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);
 
            collection.AddRange(new[] { new Element(), new Element() });

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void AddRangeFiresCollectionChanged()
        {
            var collection = new BindableCollection<Element>(new[] { new Element(), new Element() });

            var changedEvents = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (o, e) => changedEvents.Add(e);

            var elementsToAdd = new[] { new Element(), new Element() };
            collection.AddRange(elementsToAdd);

            Assert.AreEqual(1, changedEvents.Count);
            var changedEvent = changedEvents[0];
            Assert.AreEqual(NotifyCollectionChangedAction.Add, changedEvent.Action);
            Assert.AreEqual(elementsToAdd, changedEvent.NewItems);
        }

        [Test]
        public void RemoveRangeFiresPropertyChanged()
        {
            var itemsToRemove = new[] { new Element(), new Element() };
            var collection = new BindableCollection<Element>(new[] { new Element() }.Concat(itemsToRemove));

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);

            collection.RemoveRange(itemsToRemove);

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void RemoveRangeFiresCollectionChanged()
        {
            var itemsToRemove = new[] { new Element(), new Element() };
            var collection = new BindableCollection<Element>(new[] { new Element() }.Concat(itemsToRemove));

            var changedEvents = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (o, e) => changedEvents.Add(e);

            collection.AddRange(itemsToRemove);

            Assert.AreEqual(1, changedEvents.Count);
            var changedEvent = changedEvents[0];
            Assert.AreEqual(NotifyCollectionChangedAction.Add, changedEvent.Action);
            Assert.AreEqual(itemsToRemove, changedEvent.NewItems);
        }

        [Test]
        public void RefreshFiresPropertyChanged()
        {
            var collection = new BindableCollection<Element>(new[] { new Element() });

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);

            collection.Refresh();

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void RefreshFiresCollectionChanged()
        {
            var collection = new BindableCollection<Element>(new[] { new Element() });

            var changedEvents = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (o, e) => changedEvents.Add(e);

            collection.Refresh();

            Assert.AreEqual(1, changedEvents.Count);
            var changedEvent = changedEvents[0];
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, changedEvent.Action);
        }
    }
}
