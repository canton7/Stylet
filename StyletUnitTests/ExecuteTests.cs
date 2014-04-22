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
        [Test]
        public void OnUIThreadExecutesSynchronouslyIfNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            bool blockWasRun = false;
            Execute.OnUIThread(() => blockWasRun = true);
            Assert.IsTrue(blockWasRun);
        }

        [Test]
        public void OnUIThreadExecutesAsynchronouslyIfSynchronizationContextIsNotNull()
        {
            var sync = new Mock<SynchronizationContext>();
            Execute.SynchronizationContext = sync.Object;

            SendOrPostCallback passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), null)).Callback((SendOrPostCallback a, object o) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction(null);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public async Task OnUIThreadAsyncExecutesSynchronouslyIfNoSynchronizationContext()
        {
            Execute.SynchronizationContext = null;
            bool blockWasRun = false;
            await Execute.OnUIThreadAsync(() => blockWasRun = true);
            Assert.IsTrue(blockWasRun);
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
    }
}
