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
        public void OnUIThreadExecutesUsingDispatcher()
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
        public void BeginOnUIThreadExecutesUsingDispatcher()
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
        public void BeginOnUIThreadOrSynchronousExecutesUsingDispatcherIfNotCurrent()
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
        public void OnUIThreadAsyncExecutesAsynchronouslyIfDispatcherIsNotNull()
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
        public void OnUIThreadPropagatesException()
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
        public void ThrowsIfBeginOnUIThreadCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.PostToUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfBeginOnUIThreadOrSynchronousCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThreadSync(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadAsyncCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThreadAsync(() => { }));
        }

        [Test]
        public void BeginOnUIThreadExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            Execute.PostToUIThread(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void OnUIThreadExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            Execute.OnUIThreadSync(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void OnUIThreadAsyncExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
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
