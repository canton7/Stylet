using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class CommandActionTests
    {
        private class Target : PropertyChangedBase
        {
            public bool DoSomethingCalled;
            public void DoSomething()
            {
                this.DoSomethingCalled = true;
            }

            private bool _canDoSomethingWithGuard;
            public bool CanDoSomethingWithGuard
            {
                get { return this._canDoSomethingWithGuard; }
                set { SetAndNotify(ref this._canDoSomethingWithGuard, value);  }
            }
            public void DoSomethingWithGuard()
            {
            }

            public object DoSomethingArgument;
            public void DoSomethingWithArgument(object arg)
            {
                this.DoSomethingArgument = arg;
            }

            public void DoSomethingWithManyArguments(object arg1, object arg2)
            {
            }

            public object CanDoSomethingWithBadGuard
            {
                get { return false; }
            }
            public void DoSomethingWithBadGuard() { }

            public void DoSomethingUnsuccessfully()
            {
                throw new InvalidOperationException("woo");
            }

            public bool CanDoSomethingWithUnsuccessfulGuardMethod
            {
                get { throw new InvalidOperationException("foo"); }
            }

            public void DoSomethingWithUnsuccessfulGuardMethod()
            {
            }
        }

        private class Target2
        {
        }

        private DependencyObject subject;
        private Target target;

        [SetUp]
        public void SetUp()
        {
            this.target = new Target();
            this.subject = new DependencyObject();
            View.SetActionTarget(this.subject, this.target);
        }

        [Test]
        public void ThrowsIfTargetNullBehaviourIsThrowAndTargetBecomesNull()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Disable);
            Assert.Throws<ActionTargetNullException>(() => View.SetActionTarget(this.subject, null));
        }

        [Test]
        public void DisablesIfTargetNullBehaviourIsDisableAndTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, null);
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetNullBehaviourIsEnableAndTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, null);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void ThrowsIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.DoesNotThrow(() => View.SetActionTarget(this.subject, new Target2()));
            Assert.Throws<ActionNotFoundException>(() => cmd.Execute(null));
        }

        [Test]
        public void DisablesIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, new Target2());
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, new Target2());
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetAndActionExistAndNoGuardMethod()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetAndActionExistAndGuardMethodReturnsTrue()
        {
            this.target.CanDoSomethingWithGuard = true;
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void DisablesIfTargetAndActionExistAndGuardMethodReturnsFalse()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void IgnoresGuardIfGuardDoesNotReturnBool()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithBadGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(true));
        }

        [Test]
        public void ChangesEnabledStateWhenGuardChanges()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.False(cmd.CanExecute(null));
            this.target.CanDoSomethingWithGuard = true;
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void RaisesEventWhenGuardValueChanges()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            bool eventRaised = false;
            cmd.CanExecuteChanged += (o, e) => eventRaised = true;
            this.target.CanDoSomethingWithGuard = true;
            Assert.True(eventRaised);
        }

        [Test]
        public void RaisesEventWhenTargetChanges()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Disable);
            bool eventRaised = false;
            cmd.CanExecuteChanged += (o, e) => eventRaised = true;
            View.SetActionTarget(this.subject, null);
            Assert.True(eventRaised);
        }

        [Test]
        public void ExecuteDoesNothingIfTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            Assert.DoesNotThrow(() => cmd.Execute(null));
        }

        [Test]
        public void ExecuteDoesNothingIfActionIsNull()
        {
            var cmd = new CommandAction(this.subject, null, "DoesNotExist", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            Assert.DoesNotThrow(() => cmd.Execute(null));
        }

        [Test]
        public void ExecuteCallsMethod()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            cmd.Execute(null);
            Assert.True(this.target.DoSomethingCalled);
        }

        [Test]
        public void ExecutePassesArgumentIfGiven()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = "hello";
            cmd.Execute(arg);
            Assert.AreEqual("hello", this.target.DoSomethingArgument);
        }

        [Test]
        public void ThrowsIfMethodHasMoreThanOneParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new CommandAction(this.subject, null, "DoSomethingWithManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void PropagatesActionException()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomethingUnsuccessfully", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var e = Assert.Throws<InvalidOperationException>(() => cmd.Execute(null));
            Assert.AreEqual("woo", e.Message);
        }

        [Test]
        public void PropagatesGuardPropertException()
        {
            var cmd = new CommandAction(this.subject, null, "DoSomethingWithUnsuccessfulGuardMethod", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            var e = Assert.Throws<InvalidOperationException>(() => cmd.CanExecute(null));
            Assert.AreEqual("foo", e.Message);
        }

        [Test]
        public void ControlIsEnabledIfTargetIsDefault()
        {
            View.SetActionTarget(this.subject, View.InitialActionTarget);
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void ExecuteThrowsIfTargetIsDefault()
        {
            View.SetActionTarget(this.subject, View.InitialActionTarget);
            var cmd = new CommandAction(this.subject, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.Throws<ActionNotSetException>(() => cmd.Execute(null));
        }

        [Test]
        public void DoesNotRetainTarget()
        {
            var view = new DependencyObject();
            var weakView = new WeakReference(view);
            View.SetActionTarget(view, this.target);
            var cmd = new CommandAction(view, null, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            view = null;
            cmd = null;
            
            GC.Collect();

            Assert.False(weakView.IsAlive);
        }

        [Test]
        public void OperatesAfterCollection()
        {
            var view = new DependencyObject();
            var cmd = new CommandAction(view, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            GC.Collect();

            View.SetActionTarget(view, this.target);

            cmd.Execute(null);
            Assert.IsTrue(this.target.DoSomethingCalled);
        }

        [Test]
        public void UsesDataContextIfActionTargetNotAvailable()
        {
            var view = new DependencyObject();
            var backupView = new DependencyObject();
            var cmd = new CommandAction(backupView, null, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            View.SetActionTarget(backupView, this.target);
            view.SetValue(FrameworkElement.DataContextProperty, this.target);

            cmd.Execute(null);
            Assert.IsTrue(this.target.DoSomethingCalled);
        }
    }
}
