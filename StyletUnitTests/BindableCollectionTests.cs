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

        private class TestDispatcher : IDispatcher
        {
            public Action PostAction;
            public void Post(Action action)
            {
                this.PostAction = action;
            }

            public Action SendAction;
            public void Send(Action action)
            {
                this.SendAction = action;
            }

            public bool IsCurrent
            {
                get;
                set;
            }
        }

        private IDispatcher dispatcher;

        [SetUp]
        public void SetUp()
        {
            this.dispatcher = Execute.Dispatcher;
        }

        [TearDown]
        public void TearDown()
        {
            Execute.Dispatcher = this.dispatcher;
        }

        [Test]
        public void AddRangeUsesDispatcherToAddElements()
        {
            var itemsToAdd = new[] { new Element(), new Element() };
            var existingItems = new[] { new Element() };
            var collection = new BindableCollection<Element>(existingItems);

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.AddRange(itemsToAdd);

            Assert.That(collection, Is.EquivalentTo(existingItems));
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.AreEqual(existingItems.Concat(itemsToAdd), collection);
        }

        [Test]
        public void RemoveRangeUsesDispatcherToRemoveElements()
        {
            var itemsToRemove = new[] { new Element(), new Element() };
            var existingItems = new[] { new Element() };
            var collection = new BindableCollection<Element>(itemsToRemove.Concat(existingItems));

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.RemoveRange(itemsToRemove);

            Assert.That(collection, Is.EquivalentTo(itemsToRemove.Concat(existingItems)));
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

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

            collection.RemoveRange(itemsToRemove);

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
        public void RefreshUsesDispatcherToFireEvents()
        {
            var collection = new BindableCollection<Element>();

            bool propertyChangedRaised = false;
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => propertyChangedRaised = true;
            bool collectionChangedRaised = false;
            collection.CollectionChanged += (o, e) => collectionChangedRaised = true;

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.Refresh();

            Assert.False(propertyChangedRaised);
            Assert.False(collectionChangedRaised);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.True(propertyChangedRaised);
            Assert.True(collectionChangedRaised);
        }

        [Test]
        public void InsertItemUsesDispatcherToInsertItem()
        {
            var collection = new BindableCollection<Element>();

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.Add(new Element());

            Assert.AreEqual(0, collection.Count);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void InsertItemUsesDispatcherToRaiseEvents()
        {
            var collection = new BindableCollection<Element>();

            bool propertyChangedRaised = false;
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => propertyChangedRaised = true;
            bool collectionChangedRaised = false;
            collection.CollectionChanged += (o, e) => collectionChangedRaised = true;

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.Add(new Element());

            Assert.False(propertyChangedRaised);
            Assert.False(collectionChangedRaised);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.True(propertyChangedRaised);
            Assert.True(collectionChangedRaised);
        }

        [Test]
        public void SetItemUsesDispatcherToSetItems()
        {
            var initialElement = new Element();
            var collection = new BindableCollection<Element>() { initialElement };
            var element = new Element();

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection[0] = element;

            Assert.AreEqual(initialElement, collection[0]);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.AreEqual(element, collection[0]);
        }

        [Test]
        public void SetItemUsesDispatcherToRaiseEvents()
        {
            var collection = new BindableCollection<Element>() { new Element() };

            bool propertyChangedRaised = false;
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => propertyChangedRaised = true;
            bool collectionChangedRaised = false;
            collection.CollectionChanged += (o, e) => collectionChangedRaised = true;

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection[0] = new Element();

            Assert.False(propertyChangedRaised);
            Assert.False(collectionChangedRaised);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.True(propertyChangedRaised);
            Assert.True(collectionChangedRaised);
        }

        [Test]
        public void RemoveItemUsesDispatcherToRemoveItems()
        {
            var collection = new BindableCollection<Element>() { new Element() };

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.RemoveAt(0);

            Assert.AreEqual(1, collection.Count);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void RemoveItemUsesDispatcherToRaiseEvents()
        {
            var collection = new BindableCollection<Element>() { new Element() };

            bool propertyChangedRaised = false;
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => propertyChangedRaised = true;
            bool collectionChangedRaised = false;
            collection.CollectionChanged += (o, e) => collectionChangedRaised = true;

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.RemoveAt(0);

            Assert.False(propertyChangedRaised);
            Assert.False(collectionChangedRaised);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.True(propertyChangedRaised);
            Assert.True(collectionChangedRaised);
        }

        [Test]
        public void ClearItemsUsesDispatcherToClearItems()
        {
            var collection = new BindableCollection<Element>() { new Element() };

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.Clear();

            Assert.AreEqual(1, collection.Count);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void ClearItemsUsesDispatcherToRaiseEvents()
        {
            var collection = new BindableCollection<Element>() { new Element() };

            bool propertyChangedRaised = false;
            ((INotifyPropertyChanged)collection).PropertyChanged += (o, e) => propertyChangedRaised = true;
            bool collectionChangedRaised = false;
            collection.CollectionChanged += (o, e) => collectionChangedRaised = true;

            var dispatcher = new TestDispatcher();
            Execute.Dispatcher = dispatcher;

            collection.Clear();

            Assert.False(propertyChangedRaised);
            Assert.False(collectionChangedRaised);
            Assert.NotNull(dispatcher.SendAction);

            dispatcher.SendAction();

            Assert.True(propertyChangedRaised);
            Assert.True(collectionChangedRaised);
        }
    }
}
