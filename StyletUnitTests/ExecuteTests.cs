using Moq;
using NUnit.Framework;
using Stylet;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class ExecuteTests
    {
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
        public void OnUIThreadSyncExecutesUsingDispatcherIfNotCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Send(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThreadSync(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadSyncExecutesSynchronouslyIfDispatcherIsCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            sync.SetupGet(x => x.IsCurrent).Returns(true);

            bool actionCalled = false;
            Execute.OnUIThreadSync(() => actionCalled = true);

            Assert.IsTrue(actionCalled);
            sync.Verify(x => x.Send(It.IsAny<Action>()), Times.Never);
        }

        [Test]
        public void PostToUIThreadExecutesUsingDispatcher()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.PostToUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void PostToUIThreadAsyncExecutesUsingDispatcher()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            var task = Execute.PostToUIThreadAsync(() => actionCalled = true);

            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadExecutesUsingDispatcherIfNotCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadExecutesSynchronouslyIfCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            sync.SetupGet(x => x.IsCurrent).Returns(true);

            bool actionCalled = false;
            Execute.OnUIThread(() => actionCalled = true);

            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadAsyncExecutesAsynchronouslyIfNotCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            var task = Execute.OnUIThreadAsync(() => actionCalled = true);

            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadAsyncExecutesSynchronouslyIfCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            sync.SetupGet(x => x.IsCurrent).Returns(true);

            bool actionCalled = false;
            var task = Execute.OnUIThreadAsync(() => actionCalled = true);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadSyncPropagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            var ex = new Exception("testy");
            sync.Setup(x => x.Send(It.IsAny<Action>())).Callback<Action>(a => a());

            Exception caughtEx = null;
            try { Execute.OnUIThreadSync(() => { throw ex; }); }
            catch (Exception e) { caughtEx = e; }

            Assert.IsInstanceOf<System.Reflection.TargetInvocationException>(caughtEx);
            Assert.AreEqual(ex, caughtEx.InnerException);
        }

        [Test]
        public void OnUIThreadAsyncPropagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            var ex = new Exception("test");
            var task = Execute.OnUIThreadAsync(() => { throw ex; });

            passedAction();
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(ex, task.Exception.InnerExceptions[0]);
        }

        [Test]
        public void PostToUIThreadAsyncPrepagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            var ex = new Exception("test");
            var task = Execute.PostToUIThreadAsync(() => { throw ex; });

            passedAction();
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(ex, task.Exception.InnerExceptions[0]);
        }

        [Test]
        public void ThrowsIfDispatcherSetToNull()
        {
            Assert.Throws<ArgumentNullException>(() => Execute.Dispatcher = null);
        }

        [Test]
        public void DefaultDispatcherIsSynchronous()
        {
            var dispatcher = Execute.Dispatcher;

            Assert.IsTrue(dispatcher.IsCurrent);

            bool actionCalled = false;
            dispatcher.Post(() => actionCalled = true);
            Assert.IsTrue(actionCalled);

            actionCalled = false;
            dispatcher.Send(() => actionCalled = true);
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void InDesignModeIsOverridable()
        {
            try
            {
                Assert.False(Execute.InDesignMode);

                Execute.InDesignMode = true;
                Assert.True(Execute.InDesignMode);

                Execute.InDesignMode = false;
                Assert.False(Execute.InDesignMode);
            }
            finally
            {
                Execute.InDesignMode = false;
            }
        }
    }
}
