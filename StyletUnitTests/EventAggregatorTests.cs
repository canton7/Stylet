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
    public class EventAggregatorTests
    {
        public class M1 { }
        public class M2 : M1 { }

        public class C1 : IHandle<M1>
        {
            public M1 ReceivedMessage;
            public void Handle(M1 message) { this.ReceivedMessage = message; }
        }

        public class C2 : IHandle<M2>, IHandle<M1>
        {
            public M2 ReceivedM2;
            public M1 ReceivedM1;
            public void Handle(M2 message) { this.ReceivedM2 = message; }
            public void Handle(M1 message) { this.ReceivedM1 = message; }
        }

        [Test]
        public void SubscribesAndDeliversExactMessage()
        {
            var ea = new EventAggregator();
            var target = new C1();
            ea.Subscribe(target);

            var message = new M1();
            ea.Publish(message);

            Assert.AreEqual(message, target.ReceivedMessage);
        }

        [Test]
        public void DeliversToAllHandlersIncludingDerived()
        {
            var ea = new EventAggregator();
            var target = new C2();
            ea.Subscribe(target);

            var message = new M2();
            ea.Publish(message);

            Assert.AreEqual(message, target.ReceivedM1);
            Assert.AreEqual(message, target.ReceivedM2);
        }
    }
}
