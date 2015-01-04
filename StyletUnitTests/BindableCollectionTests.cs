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

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            Execute.TestExecuteSynchronously = true;
        }

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
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, changedEvent.Action);
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
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, changedEvent.Action);
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

        [Test]
        public void PropertyChangedDispatcherDefaultsToExecuteDefaultPropertyChangedDispatcher()
        {
            var oldDispatcher = Execute.DefaultPropertyChangedDispatcher;
            Execute.DefaultPropertyChangedDispatcher = a => a();

            var collection = new BindableCollection<Element>();

            Assert.AreEqual(collection.PropertyChangedDispatcher, Execute.DefaultPropertyChangedDispatcher);

            Execute.DefaultPropertyChangedDispatcher = oldDispatcher;
        }

        [Test]
        public void CollectionChangedDispatcherDefaultsToExecuteDefaultCollectionChangedDispatcher()
        {
            var oldDispatcher = Execute.DefaultCollectionChangedDispatcher;
            Execute.DefaultCollectionChangedDispatcher = a => a();

            var collection = new BindableCollection<Element>();

            Assert.AreEqual(collection.CollectionChangedDispatcher, Execute.DefaultCollectionChangedDispatcher);

            Execute.DefaultCollectionChangedDispatcher = oldDispatcher;
        }

        [Test]
        public void UsesPropertyChangedDipatcher()
        {
            var collection = new BindableCollection<Element>();

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => changedProperties.Add(e.PropertyName);

            var dispatchedActions = new List<Action>();
            collection.PropertyChangedDispatcher = a => dispatchedActions.Add(a);

            collection.Add(new Element());

            Assert.IsEmpty(changedProperties);
            Assert.IsNotEmpty(dispatchedActions);

            foreach (var action in dispatchedActions)
            {
                action();
            }

            Assert.That(changedProperties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void UsesCollectionChangedDispatcher()
        {
            var collection = new BindableCollection<Element>();

            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (o, e) => events.Add(e);

            var dispatchedActions = new List<Action>();
            collection.CollectionChangedDispatcher = a => dispatchedActions.Add(a);

            collection.Add(new Element());

            Assert.IsEmpty(events);
            Assert.IsNotEmpty(dispatchedActions);

            foreach (var action in dispatchedActions)
            {
                action();
            }

            Assert.AreEqual(1, events.Count);
            var changedEvent = events[0];
            Assert.AreEqual(NotifyCollectionChangedAction.Add, changedEvent.Action);
        }
    }
}
