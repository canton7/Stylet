using NUnit.Framework;
using Stylet;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class EventAggregatorTests
    {
        public class M1 { }
        public class M2 : M1 { }

        public class C1 : IHandle<M1>
        {
            public M1 ReceivedMessage;
            public int ReceivedMessageCount;
            public void Handle(M1 message) { this.ReceivedMessage = message; this.ReceivedMessageCount++; }
        }

        public class C2 : IHandle<M2>, IHandle<M1>
        {
            public M2 ReceivedM2;
            public M1 ReceivedM1;
            public void Handle(M2 message) { this.ReceivedM2 = message; }
            public void Handle(M1 message) { this.ReceivedM1 = message; }
        }

        public class C3 : IHandle<M1>
        {
            public void Handle(M1 message) { throw new Exception("Should not be called. Ever"); }
        }

        public class C4 : IHandle<M1>
        {
            public EventAggregator EventAggregator;

            public void Handle(M1 message)
            {
                this.EventAggregator.Publish("foo");
            }
        }

        public class C5 : IHandle<M1>
        {
            public EventAggregator EventAggregator;

            public void Handle(M1 message)
            {
                this.EventAggregator.Subscribe(new C4());
            }
        }

        private EventAggregator ea;

        [SetUp]
        public void SetUp()
        {
            this.ea = new EventAggregator();
        }

        [Test]
        public void SubscribesAndDeliversExactMessage()
        {
            var target = new C1();
            this.ea.Subscribe(target);

            var message = new M1();
            this.ea.Publish(message);

            Assert.AreEqual(message, target.ReceivedMessage);
        }

        [Test]
        public void DeliversToAllHandlersIncludingDerived()
        {
            var target = new C2();
            this.ea.Subscribe(target);

            var message = new M2();
            this.ea.Publish(message);

            Assert.AreEqual(message, target.ReceivedM1);
            Assert.AreEqual(message, target.ReceivedM2);
        }

        [Test]
        public void UnsubscribeUnsubscribes()
        {
            var target = new C1();
            this.ea.Subscribe(target);
            this.ea.Unsubscribe(target);

            var message = new M1();
            this.ea.Publish(message);

            Assert.IsNull(target.ReceivedMessage);
        }

        [Test]
        public void TargetReferenceIsWeak()
        {
            var target = new C3();
            var weaktarget = new WeakReference(target);
            this.ea.Subscribe(target);

            // Ugly, but it's the only way to test a WeakReference...
            target = null;
            GC.Collect();

            Assert.DoesNotThrow(() => this.ea.Publish(new M1()));
            Assert.IsNull(weaktarget.Target);
        }

        [Test]
        public void SubscribingTwiceDoesNothing()
        {
            var target = new C1();
            this.ea.Subscribe(target);
            this.ea.Subscribe(target);

            var message = new M1();
            this.ea.Publish(message);

            Assert.AreEqual(1, target.ReceivedMessageCount);
        }

        [Test]
        public void PublishOnUIThreadPublishedOnUIThread()
        {
            var target = new C1();
            this.ea.Subscribe(target);

            var message = new M1();
            this.ea.PublishOnUIThread(message);

            Assert.AreEqual(message, target.ReceivedMessage);
        }

        [Test]
        public void TargetReceivesMessagesOnTopicsItIsSubscribedTo()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1", "C2");

            var message = new M1();
            this.ea.Publish(message, "C1");
            this.ea.Publish(message, "C2");

            Assert.AreEqual(2, target.ReceivedMessageCount);
        }

        [Test]
        public void TargetDoesNotReceiveMessagesOnTopicsItIsNotSubscribedTo()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1");

            var message = new M1();
            this.ea.Publish(message, "C2");

            Assert.AreEqual(0, target.ReceivedMessageCount);
        }

        [Test]
        public void SubscribesToDefaultChannelByDefault()
        {
            var target = new C1();
            this.ea.Subscribe(target);

            var message = new M1();
            this.ea.Publish(message, EventAggregator.DefaultChannel);

            Assert.AreEqual(message, target.ReceivedMessage);
        }

        [Test]
        public void DoesNotSubscribeToDefaultChannelIfAChannelIsGiven()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1");

            var message = new M1();
            this.ea.Publish(message);

            Assert.AreEqual(0, target.ReceivedMessageCount);
        }

        [Test]
        public void DoesNotPublishToDefaultChannelIfAChannelIsGiven()
        {
            var target = new C1();
            this.ea.Subscribe(target);

            var message = new M1();
            this.ea.Publish(message, "C1");

            Assert.AreEqual(0, target.ReceivedMessageCount);
        }

        [Test]
        public void AdditionalChannelsCanBeSubscribedTo()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1");
            this.ea.Subscribe(target, "C2");

            var message = new M1();
            this.ea.Publish(message, "C1");
            this.ea.Publish(message, "C2");

            Assert.AreEqual(2, target.ReceivedMessageCount);
        }

        [Test]
        public void IndividualChannelsCanBeUnsubscribedFrom()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1", "C2");
            this.ea.Unsubscribe(target, "C1");

            var message = new M1();
            this.ea.Publish(message, "C1");
            Assert.AreEqual(0, target.ReceivedMessageCount);

            this.ea.Publish(message, "C2");
            Assert.AreEqual(1, target.ReceivedMessageCount);
        }

        [Test]
        public void UnsubscribeUnsubscribesFromEverythingIfNoChannelsGiven()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1", "C2");
            this.ea.Unsubscribe(target);

            var message = new M1();
            this.ea.Publish(message, "C1", "C2");

            Assert.AreEqual(0, target.ReceivedMessageCount);
        }

        [Test]
        public void MessagePublishedToMultipleChannelsGetsDeliveredOnce()
        {
            var target = new C1();
            this.ea.Subscribe(target, "C1", "C2");

            var message = new M1();
            this.ea.Publish(message, "C1", "C2");

            Assert.AreEqual(1, target.ReceivedMessageCount);
        }

        [Test]
        public void PublishingInsideHandlerDoesNotThrow()
        {
            var target = new C4();
            target.EventAggregator = this.ea;
            this.ea.Subscribe(target);

            // Add this as a dummy - it has to be a dead reference, which triggers modification of the
            // 'handlers' collection
            var dummyTarget = new C1();
            this.ea.Subscribe(dummyTarget);
            dummyTarget = null;
            GC.Collect();

            Assert.DoesNotThrow(() => this.ea.Publish(new M1()));

            GC.KeepAlive(target);
        }

        [Test]
        public void SubscribingInsideHandlerDoesNotThrow()
        {
            var target = new C5();
            target.EventAggregator = this.ea;
            this.ea.Subscribe(target);

            this.ea.Publish(new M1());
        }
    }
}
