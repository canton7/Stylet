using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class ScreenTests
    {
        private class MyScreen : Screen
        {
            public IValidatorAdapter Validator
            {
                get { return base.validator; }
                set { base.validator = value; }
            }

            public IWeakEventManager WeakEventManager
            {
                get { return base.weakEventManager; }
            }

            public MyScreen() { }
            public MyScreen(IValidatorAdapter validator) : base(validator) { }

            public bool OnActivateCalled;
            protected override void OnActivate()
            {
                this.OnActivateCalled = true;
            }

            public bool OnInitialActivateCalled;
            protected override void OnInitialActivate()
            {
                this.OnInitialActivateCalled = true;
            }

            public bool OnDeactivateCalled;
            protected override void OnDeactivate()
            {
                this.OnDeactivateCalled = true;
            }

            public bool OnCloseCalled;
            protected override void OnClose()
            {
                this.OnCloseCalled = true;
            }

            public bool OnViewLoadedCalled;
            protected override void OnViewLoaded()
            {
                this.OnViewLoadedCalled = true;
            }
        }

        private class WeakEventScreen : Screen
        {
            public IWeakEventManager WeakEventManager;
            protected override IWeakEventManager weakEventManager
            {
                get { return this.WeakEventManager; }
            }

            public new IEventBinding BindWeak<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> selector, Action<TProperty> handler)
                where TSource : class, INotifyPropertyChanged
            {
                return base.BindWeak(source, selector, handler);
            }
        }

        private MyScreen screen;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            Execute.TestExecuteSynchronously = true;
        }


        [SetUp]
        public void SetUp()
        {
            this.screen = new MyScreen();
        }

        [Test]
        public void SettingDisplayNameNotifies()
        {
            string changedProperty = null;
            this.screen.PropertyChanged += (o, e) => changedProperty = e.PropertyName;

            this.screen.DisplayName = "test";

            Assert.AreEqual("test", this.screen.DisplayName);
            Assert.AreEqual("DisplayName", changedProperty);
        }

        [Test]
        public void ScreenIsInitiallyNotActive()
        {
            Assert.IsFalse(this.screen.IsActive);
        }

        [Test]
        public void ActivateActivatesIfNotAlreadyActive()
        {
            ((IActivate)this.screen).Activate();
            Assert.IsTrue(this.screen.IsActive);
        }

        [Test]
        public void ActivateFiresActivatedEvent()
        {
            bool fired = false;
            this.screen.Activated += (o, e) => fired = true;
            ((IActivate)this.screen).Activate();
            Assert.IsTrue(fired);
        }

        [Test]
        public void ActivateCallsOnActivate()
        {
            ((IActivate)this.screen).Activate();
            Assert.IsTrue(this.screen.OnActivateCalled);
        }

        [Test]
        public void DoubleActivationDoesntActivate()
        {
            ((IActivate)this.screen).Activate();
            this.screen.OnActivateCalled = false;
            ((IActivate)this.screen).Activate();
            Assert.IsFalse(this.screen.OnActivateCalled);
        }

        [Test]
        public void InitialActivationCallsOnInitialActivate()
        {
            ((IActivate)this.screen).Activate();
            this.screen.OnInitialActivateCalled = false;
            ((IDeactivate)this.screen).Deactivate();
            ((IActivate)this.screen).Activate();
            Assert.IsFalse(this.screen.OnInitialActivateCalled);
        }

        [Test]
        public void DeactivateDeactivates()
        {
            ((IActivate)this.screen).Activate(); ;
            ((IDeactivate)this.screen).Deactivate();
            Assert.IsFalse(this.screen.IsActive);
        }

        [Test]
        public void DeactivateFiredDeactivatedEvent()
        {
            bool fired = false;
            this.screen.Deactivated += (o, e) => fired = true;
            ((IActivate)this.screen).Activate(); ;
            ((IDeactivate)this.screen).Deactivate();
            Assert.IsTrue(fired);
        }

        [Test]
        public void DeactivateCallsOnDeactivate()
        {
            ((IActivate)this.screen).Activate();
            ((IDeactivate)this.screen).Deactivate();
            Assert.IsTrue(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void DoubleDeactivationDoesntDeactivate()
        {
            ((IActivate)this.screen).Activate();
            ((IDeactivate)this.screen).Deactivate();
            this.screen.OnDeactivateCalled = false;
            ((IDeactivate)this.screen).Deactivate();
            Assert.IsFalse(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void CloseDeactivates()
        {
            ((IActivate)this.screen).Activate();
            ((IClose)this.screen).Close();
            Assert.IsTrue(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void CloseClearsView()
        {
            ((IViewAware)this.screen).AttachView(new UIElement());
            ((IClose)this.screen).Close();
            Assert.IsNull(this.screen.View);
        }

        [Test]
        public void CloseFiresClosed()
        {
            bool fired = false;
            this.screen.Closed += (o, e) => fired = true;
            ((IClose)this.screen).Close();
            Assert.IsTrue(fired);
        }

        [Test]
        public void CloseCallsOnClose()
        {
            ((IClose)this.screen).Close();
            Assert.IsTrue(this.screen.OnCloseCalled);
        }

        [Test]
        public void AttachViewAttachesView()
        {
            var view = new UIElement();
            ((IViewAware)this.screen).AttachView(view);
            Assert.AreEqual(view, this.screen.View);
        }

        [Test]
        public void AttachViewThrowsIfViewAlreadyAttached()
        {
            var view = new UIElement();
            ((IViewAware)this.screen).AttachView(view);
            Assert.Throws<Exception>(() => ((IViewAware)this.screen).AttachView(view));
        }

        [Test]
        public void SettingParentRaisesPropertyChange()
        {
            var parent = new object();
            string changedProperty = null;
            this.screen.PropertyChanged += (o, e) => changedProperty = e.PropertyName;
            this.screen.Parent = parent;

            Assert.AreEqual(parent, this.screen.Parent);
            Assert.AreEqual("Parent", changedProperty);
        }

        [Test]
        public void CanCloseAsyncReturnsCompletedTrueTask()
        {
            var task = this.screen.CanCloseAsync();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.Result);
        }

        [Test]
        public void TryCloseThrowsIfParentIsNotIChildDelegate()
        {
            this.screen.Parent = new object();
            Assert.Throws<InvalidOperationException>(() => this.screen.TryClose());
        }

        [Test]
        public void TryCloseCallsParentCloseItemPassingDialogResult()
        {
            var parent = new Mock<IChildDelegate>();
            screen.Parent = parent.Object;
            this.screen.TryClose(true);
            parent.Verify(x => x.CloseItem(this.screen, true));
        }

        [Test]
        public void PassesValidatorAdapter()
        {
            var adapter = new Mock<IValidatorAdapter>();
            var screen = new MyScreen(adapter.Object);
            Assert.AreEqual(adapter.Object, screen.Validator);
        }

        [Test]
        public void WeakEventManagerReturnsConsistentObject()
        {
            var w1 = screen.WeakEventManager;
            var w2 = screen.WeakEventManager;
            Assert.AreEqual(w1, w2);
        }

        [Test]
        public void BindWeakProxies()
        {
            var s = new WeakEventScreen();
            var m = new Mock<IWeakEventManager>();
            s.WeakEventManager = m.Object;

            var source = new LabelledValue<int>("test", 5);
            Expression<Func<LabelledValue<int>, int>> selector = x => x.Value;
            Action<int> handler = x => { };
            s.BindWeak(source, selector, handler);

            m.Verify(x => x.BindWeak(source, selector, handler));
        }
    }
}
