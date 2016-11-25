using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class ScreenTests
    {
        private class MyScreen : Screen
        {
            public new IModelValidator Validator
            {
                get { return base.Validator; }
                set { base.Validator = value; }
            }

            public MyScreen() { }
            public MyScreen(IModelValidator validator) : base(validator) { }

            public void Reset()
            {
                this.OnActivateCalled = false;
                this.OnInitialActivateCalled = false;
                this.OnDeactivateCalled = false;
                this.OnCloseCalled = false;
            }

            public new void SetState(ScreenState newState, Action<ScreenState, ScreenState> changedHandler)
            {
                base.SetState(newState, changedHandler);
            }

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

            public ScreenState PreviousState;
            public ScreenState NewState;
            protected override void OnStateChanged(ScreenState oldState, ScreenState newState)
            {
                this.PreviousState = oldState;
                this.NewState = newState;
            }

            public bool OnViewLoadedCalled;
            protected override void OnViewLoaded()
            {
                this.OnViewLoadedCalled = true;
            }

            public bool? CanCloseResult = null;
            public override Task<bool> CanCloseAsync()
            {
                return this.CanCloseResult == null ? base.CanCloseAsync() : Task.FromResult(this.CanCloseResult.Value);
            }
        }

        private MyScreen screen;

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
        public void ScreenIsInitiallyDeactivated()
        {
            Assert.AreEqual(ScreenState.Deactivated, this.screen.ScreenState);
        }

        [Test]
        public void ActivateActivatesIfNotAlreadyActive()
        {
            ((IScreenState)this.screen).Activate();
            Assert.IsTrue(this.screen.IsActive);
        }

        [Test]
        public void ActivateFiresActivatedEvent()
        {
            bool fired = false;
            this.screen.Activated += (o, e) => fired = true;
            ((IScreenState)this.screen).Activate();
            Assert.IsTrue(fired);
        }

        [Test]
        public void ActivateCallsOnActivate()
        {
            ((IScreenState)this.screen).Activate();
            Assert.IsTrue(this.screen.OnActivateCalled);
        }

        [Test]
        public void DoubleActivationDoesntActivate()
        {
            ((IScreenState)this.screen).Activate();
            this.screen.OnActivateCalled = false;
            ((IScreenState)this.screen).Activate();
            Assert.IsFalse(this.screen.OnActivateCalled);
        }

        [Test]
        public void InitialActivationCallsOnInitialActivate()
        {
            ((IScreenState)this.screen).Activate();
            this.screen.OnInitialActivateCalled = false;
            ((IScreenState)this.screen).Deactivate();
            ((IScreenState)this.screen).Activate();
            Assert.IsFalse(this.screen.OnInitialActivateCalled);
        }

        [Test]
        public void ActivateFiresCorrectEvents()
        {
            // Get the initial activate out of the way
            ((IScreenState)this.screen).Activate();
            this.screen.SetState(ScreenState.Deactivated, (n, o) => { });
            
            var changedEventArgs = new List<ScreenStateChangedEventArgs>();
            this.screen.StateChanged += (o, e) => changedEventArgs.Add(e);
            var activatedEventArgs = new List<ActivationEventArgs>();
            this.screen.Activated += (o, e) => activatedEventArgs.Add(e);

            ((IScreenState)this.screen).Activate();

            Assert.AreEqual(1, changedEventArgs.Count);
            Assert.AreEqual(ScreenState.Active, changedEventArgs[0].NewState);
            Assert.AreEqual(ScreenState.Deactivated, changedEventArgs[0].PreviousState);

            Assert.AreEqual(ScreenState.Deactivated, this.screen.PreviousState);
            Assert.AreEqual(ScreenState.Active, this.screen.NewState);

            Assert.AreEqual(1, activatedEventArgs.Count);
            Assert.AreEqual(ScreenState.Deactivated, activatedEventArgs[0].PreviousState);
            Assert.IsFalse(activatedEventArgs[0].IsInitialActivate);
        }

        [Test]
        public void InitialActivateFiresCorrectEvents()
        {
            var changedEventArgs = new List<ScreenStateChangedEventArgs>();
            this.screen.StateChanged += (o, e) => changedEventArgs.Add(e);
            var activatedEventArgs = new List<ActivationEventArgs>();
            this.screen.Activated += (o, e) => activatedEventArgs.Add(e);

            ((IScreenState)this.screen).Activate();

            Assert.AreEqual(1, changedEventArgs.Count);
            Assert.AreEqual(ScreenState.Active, changedEventArgs[0].NewState);
            Assert.AreEqual(ScreenState.Deactivated, changedEventArgs[0].PreviousState);

            Assert.AreEqual(ScreenState.Deactivated, this.screen.PreviousState);
            Assert.AreEqual(ScreenState.Active, this.screen.NewState);

            Assert.AreEqual(1, activatedEventArgs.Count);
            Assert.AreEqual(ScreenState.Deactivated, activatedEventArgs[0].PreviousState);
            Assert.IsTrue(activatedEventArgs[0].IsInitialActivate);
        }

        [Test]
        public void DeactivateDeactivates()
        {
            ((IScreenState)this.screen).Activate(); ;
            ((IScreenState)this.screen).Deactivate();
            Assert.IsFalse(this.screen.IsActive);
        }

        [Test]
        public void DeactivateFiredDeactivatedEvent()
        {
            bool fired = false;
            this.screen.Deactivated += (o, e) => fired = true;
            ((IScreenState)this.screen).Activate(); ;
            ((IScreenState)this.screen).Deactivate();
            Assert.IsTrue(fired);
        }

        [Test]
        public void DeactivateCallsOnDeactivate()
        {
            ((IScreenState)this.screen).Activate();
            ((IScreenState)this.screen).Deactivate();
            Assert.IsTrue(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void DoubleDeactivationDoesntDeactivate()
        {
            ((IScreenState)this.screen).Activate();
            ((IScreenState)this.screen).Deactivate();
            this.screen.OnDeactivateCalled = false;
            ((IScreenState)this.screen).Deactivate();
            Assert.IsFalse(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void DeactivateFiresCorrectEvents()
        {
            this.screen.SetState(ScreenState.Active, (n, o) => { });

            var changedEventArgs = new List<ScreenStateChangedEventArgs>();
            this.screen.StateChanged += (o, e) => changedEventArgs.Add(e);
            var deactivationEventArgs = new List<DeactivationEventArgs>();
            this.screen.Deactivated += (o, e) => deactivationEventArgs.Add(e);

            ((IScreenState)this.screen).Deactivate();

            Assert.AreEqual(1, changedEventArgs.Count);
            Assert.AreEqual(ScreenState.Deactivated, changedEventArgs[0].NewState);
            Assert.AreEqual(ScreenState.Active, changedEventArgs[0].PreviousState);

            Assert.AreEqual(ScreenState.Active, this.screen.PreviousState);
            Assert.AreEqual(ScreenState.Deactivated, this.screen.NewState);

            Assert.AreEqual(1, deactivationEventArgs.Count);
            Assert.AreEqual(ScreenState.Active, deactivationEventArgs[0].PreviousState);
        }

        [Test]
        public void CloseDeactivates()
        {
            ((IScreenState)this.screen).Activate();
            ((IScreenState)this.screen).Close();
            Assert.IsTrue(this.screen.OnDeactivateCalled);
        }

        [Test]
        public void CloseClearsView()
        {
            ((IViewAware)this.screen).AttachView(new UIElement());
            ((IScreenState)this.screen).Close();
            Assert.IsNull(this.screen.View);
        }

        [Test]
        public void CloseFiresClosed()
        {
            bool fired = false;
            this.screen.Closed += (o, e) => fired = true;
            ((IScreenState)this.screen).Close();
            Assert.IsTrue(fired);
        }

        [Test]
        public void CloseCallsOnClose()
        {
            ((IScreenState)this.screen).Close();
            Assert.IsTrue(this.screen.OnCloseCalled);
        }

        [Test]
        public void DoubleCloseDoesNotClose()
        {
            ((IScreenState)this.screen).Close();
            this.screen.OnCloseCalled = false;
            ((IScreenState)this.screen).Close();
            Assert.IsFalse(this.screen.OnCloseCalled);
        }

        [Test]
        public void CloseFiresCorrectEvents()
        {
            this.screen.SetState(ScreenState.Deactivated, (n, o) => { });

            var changedEventArgs = new List<ScreenStateChangedEventArgs>();
            this.screen.StateChanged += (o, e) => changedEventArgs.Add(e);
            var closeEventArgs = new List<CloseEventArgs>();
            this.screen.Closed += (o, e) => closeEventArgs.Add(e);

            ((IScreenState)this.screen).Close();

            Assert.AreEqual(1, changedEventArgs.Count);
            Assert.AreEqual(ScreenState.Closed, changedEventArgs[0].NewState);
            Assert.AreEqual(ScreenState.Deactivated, changedEventArgs[0].PreviousState);

            Assert.AreEqual(ScreenState.Deactivated, this.screen.PreviousState);
            Assert.AreEqual(ScreenState.Closed, this.screen.NewState);

            Assert.AreEqual(1, closeEventArgs.Count);
            Assert.AreEqual(ScreenState.Deactivated, closeEventArgs[0].PreviousState);
        }

        [Test]
        public void ActivatingAllowsScreenToBeClosedAgain()
        {
            ((IScreenState)this.screen).Close();
            this.screen.OnCloseCalled = false;
            ((IScreenState)this.screen).Activate();
            ((IScreenState)this.screen).Close();
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
            Assert.Throws<InvalidOperationException>(() => ((IViewAware)this.screen).AttachView(view));
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
        public void CanCloseAsyncReturnsTrueByDefault()
        {
            var task = this.screen.CanCloseAsync();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.Result);
        }

        [Test]
        public void CanCloseAsyncReturnsResultOfCanClose()
        {
            this.screen.CanCloseResult = false;
            var task = this.screen.CanCloseAsync();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.Result);
        }

        [Test]
        public void RequestCloseThrowsIfParentIsNotIChildDelegate()
        {
            this.screen.Parent = new object();
            Assert.Throws<InvalidOperationException>(() => this.screen.RequestClose());
        }

        [Test]
        public void RequestCloseCallsParentCloseItemPassingDialogResult()
        {
            var parent = new Mock<IChildDelegate>();
            screen.Parent = parent.Object;
            this.screen.RequestClose(true);
            parent.Verify(x => x.CloseItem(this.screen, true));
        }

        // OBSELETED - but need to test anyway...

#pragma warning disable 618

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

#pragma warning restore 618

        [Test]
        public void PassesValidatorAdapter()
        {
            var adapter = new Mock<IModelValidator>();
            var screen = new MyScreen(adapter.Object);
            Assert.AreEqual(adapter.Object, screen.Validator);
        }

        [Test]
        public void InitialActivateFiredWhenComingFromDeactivated()
        {
            ((IScreenState)this.screen).Deactivate();
            ((IScreenState)this.screen).Activate();
            Assert.True(this.screen.OnInitialActivateCalled);
        }

        [Test]
        public void ClosingResetsInitialActivate()
        {
            ((IScreenState)this.screen).Activate();
            this.screen.OnInitialActivateCalled = false;
            ((IScreenState)this.screen).Close();
            ((IScreenState)this.screen).Activate();
            Assert.True(this.screen.OnInitialActivateCalled);
        }

        [Test]
        public void DeactivateAfterCloseCausesActivate()
        {
            ((IScreenState)this.screen).Activate();
            ((IScreenState)this.screen).Close();
            this.screen.Reset();

            ((IScreenState)this.screen).Deactivate();
            Assert.True(this.screen.OnInitialActivateCalled);
            Assert.True(this.screen.OnActivateCalled);
            Assert.True(this.screen.OnDeactivateCalled);
        }
    }
}
