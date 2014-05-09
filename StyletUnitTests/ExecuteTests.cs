using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ExecuteTests
    {
        [SetUp]
        public void SetUp()
        {
            // Dont want this being previously set by anything and messing us around
            Execute.TestExecuteSynchronously = false;
        }

        [Test]
        public void OnUIThreadExecutesUsingSynchronizationContext()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Send(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction(null);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void BeginOnUIThreadExecutesUsingSynchronizationContext()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            bool actionCalled = false;
            Execute.BeginOnUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction(null);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void BeginOnUIThreadOrSynchronousExecutesUsingSynchronizationContextIfNotCurrent()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            bool actionCalled = false;
            Execute.BeginOnUIThreadOrSynchronous(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction(null);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadAsyncExecutesAsynchronouslyIfSynchronizationContextIsNotNull()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            bool actionCalled = false;
            var task = Execute.OnUIThreadAsync(() => actionCalled = true);

            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(actionCalled);
            passedAction(null);
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadPropagatesException()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            var ex = new Exception("testy");
            sync.Setup(x => x.Send(It.IsAny<SendOrPostCallback>(), null)).Callback<SendOrPostCallback, object>((a, b) => a(b));

            Exception caughtEx = null;
            try { Execute.OnUIThread(() => { throw ex; }); }
            catch (Exception e) { caughtEx = e; }

            Assert.IsInstanceOf<System.Reflection.TargetInvocationException>(caughtEx);
            Assert.AreEqual(ex, caughtEx.InnerException);
        }

        [Test]
        public void OnUIThreadAsyncPropagatesException()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            var ex = new Exception("test");
            var task = Execute.OnUIThreadAsync(() => { throw ex; });

            passedAction(null);
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(ex, task.Exception.InnerExceptions[0]);
        }

        [Test]
        public void ThrowsIfBeginOnUIThreadCalledWithNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            Assert.Throws<InvalidOperationException>(() => Execute.BeginOnUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfBeginOnUIThreadOrSynchronousCalledWithNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            Assert.Throws<InvalidOperationException>(() => Execute.BeginOnUIThreadOrSynchronous(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadCalledWithNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadAsyncCalledWithNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThreadAsync(() => { }));
        }

        [Test]
        public void BeginOnUIThreadExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.SynchronizationContext = null;
            bool called = false;
            Execute.BeginOnUIThread(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void OnUIThreadExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.SynchronizationContext = null;
            bool called = false;
            Execute.OnUIThread(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void OnUIThreadAsyncExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.SynchronizationContext = null;
            bool called = false;
            Execute.OnUIThreadAsync(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void InDesignModeReturnsFalse()
        {
            Assert.False(Execute.InDesignMode);
        }
    }
}
