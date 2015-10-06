using NUnit.Framework;
using Stylet.Xaml;
using System;
using System.Reflection;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class EventActionTests
    {
        private class Subject : DependencyObject
        {
            // They're used by reflection - squish 'unused' warning
#pragma warning disable 0067
            public event EventHandler SimpleEventHandler;
            public event Action BadEventHandler;
            public event DependencyPropertyChangedEventHandler DependencyChangedEventHandler;
#pragma warning restore 0067
        }

        private class Target
        {
            public bool DoSomethingCalled;
            public void DoSomething()
            {
                this.DoSomethingCalled = true;
            }

            public void DoSomethingWithBadArgument(string arg)
            {
            }

            public void DoSomethingWithSenderAndBadArgument(object sender, object e)
            {
            }

            public void DoSomethingWithTooManyArguments(object sender, EventArgs e, object another)
            {
            }

            public EventArgs EventArgs;
            public void DoSomethingWithEventArgs(EventArgs ea)
            {
                this.EventArgs = ea;
            }

            public object Sender;
            public void DoSomethingWithObjectAndEventArgs(object sender, EventArgs e)
            {
                this.Sender = sender;
                this.EventArgs = e;
            }

            public DependencyPropertyChangedEventArgs DependencyChangedEventArgs;
            public void DoSomethingWithDependencyChangedEventArgs(DependencyPropertyChangedEventArgs e)
            {
                this.DependencyChangedEventArgs = e;
            }

            public void DoSomethingWithObjectAndDependencyChangedEventArgs(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.Sender = sender;
                this.DependencyChangedEventArgs = e;
            }

            public void DoSomethingUnsuccessfully()
            {
                throw new InvalidOperationException("foo");
            }
        }

        private class Target2
        {
        }

        private DependencyObject subject;
        private Target target;
        private EventInfo eventInfo;
        private EventInfo dependencyChangedEventInfo;

        [SetUp]
        public void SetUp()
        {
            this.target = new Target();
            this.subject = new Subject();
            this.eventInfo = typeof(Subject).GetEvent("SimpleEventHandler");
            this.dependencyChangedEventInfo = typeof(Subject).GetEvent("DependencyChangedEventHandler");
            View.SetActionTarget(this.subject, this.target);
        }

        [Test]
        public void ThrowsIfNullTargetBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfNonExistentActionBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Disable));
        }

        [Test]
        public void ThrowsIfTargetNullBehaviourIsThrowAndTargetBecomesNull()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Enable);
            Assert.Throws<ActionTargetNullException>(() => View.SetActionTarget(this.subject, null));
        }

        [Test]
        public void ThrowsWhenClickedIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Throw);
            Assert.DoesNotThrow(() => View.SetActionTarget(this.subject, new Target2()));
            var e = Assert.Throws<TargetInvocationException>(() => cmd.GetDelegate().DynamicInvoke(null, new RoutedEventArgs()));
            Assert.IsInstanceOf<ActionNotFoundException>(e.InnerException);
        }

        [Test]
        public void ThrowsIfMethodHasTooManyArguments()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithTooManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasBadParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithBadArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasBadEventArgsParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithSenderAndBadArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasTooManyParameters()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithTooManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void InvokingCommandDoesNothingIfTargetIsNull()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandDoesNothingIfActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, new Target2());
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandCallsMethod()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            cmd.GetDelegate().DynamicInvoke(null, null);
            Assert.True(this.target.DoSomethingCalled);
        }

        [Test]
        public void InvokingCommandCallsMethodWithEventArgs()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = new RoutedEventArgs();
            cmd.GetDelegate().DynamicInvoke(null, arg);
            Assert.AreEqual(arg, this.target.EventArgs);
        }

        [Test]
        public void InvokingCommandCallsMethodWithSenderAndEventArgs()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingWithObjectAndEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var sender = new object();
            var arg = new RoutedEventArgs();
            cmd.GetDelegate().DynamicInvoke(sender, arg);

            Assert.AreEqual(sender, this.target.Sender);
            Assert.AreEqual(arg, this.target.EventArgs);
        }

        [Test]
        public void InvokingCommandCallsMethodWithDependencyChangedEventArgs()
        {
            var cmd = new EventAction(this.subject, null, this.dependencyChangedEventInfo.EventHandlerType, "DoSomethingWithDependencyChangedEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = new DependencyPropertyChangedEventArgs();
            cmd.GetDelegate().DynamicInvoke(null, arg);
            Assert.AreEqual(arg, this.target.DependencyChangedEventArgs);
        }

        [Test]
        public void InvokingCommandCallsMethodWithSenderAndDependencyChangedEventArgs()
        {
            var cmd = new EventAction(this.subject, null, this.dependencyChangedEventInfo.EventHandlerType, "DoSomethingWithObjectAndDependencyChangedEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var sender = new object();
            var arg = new DependencyPropertyChangedEventArgs();
            cmd.GetDelegate().DynamicInvoke(sender, arg);

            Assert.AreEqual(sender, this.target.Sender);
            Assert.AreEqual(arg, this.target.DependencyChangedEventArgs);
        }

        [Test]
        public void BadEventHandlerSignatureThrows()
        {
            var cmd = new EventAction(this.subject, null, typeof(Subject).GetEvent("BadEventHandler").EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            Assert.Throws<ActionEventSignatureInvalidException>(() => cmd.GetDelegate());
        }

        [Test]
        public void PropagatesActionException()
        {
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomethingUnsuccessfully", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var e = Assert.Throws<TargetInvocationException>(() => cmd.GetDelegate().DynamicInvoke(null, null));
            Assert.IsInstanceOf<InvalidOperationException>(e.InnerException);
            Assert.AreEqual("foo", e.InnerException.Message);
        }

        [Test]
        public void ExecuteThrowsIfActionTargetIsDefault()
        {
            View.SetActionTarget(this.subject, View.InitialActionTarget);
            var cmd = new EventAction(this.subject, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            var e = Assert.Throws<TargetInvocationException>(() => cmd.GetDelegate().DynamicInvoke(null, null));
            Assert.IsInstanceOf<ActionNotSetException>(e.InnerException);
        }

        [Test]
        public void DoesNotRetainTarget()
        {
            var view = new DependencyObject();
            var weakView = new WeakReference(view);
            View.SetActionTarget(view, this.target);
            var cmd = new EventAction(view, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            cmd = null;
            view = null;
            GC.Collect();

            Assert.IsFalse(weakView.IsAlive);
        }

        [Test]
        public void OperatesAfterCollection()
        {
            var view = new DependencyObject();
            var cmd = new EventAction(view, null, this.eventInfo.EventHandlerType, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            GC.Collect();

            View.SetActionTarget(view, this.target);

            cmd.GetDelegate().DynamicInvoke(null, null);
            Assert.IsTrue(this.target.DoSomethingCalled);
        }

        [Test]
        public void UsesBackupSubjectIfActionTargetNotAvailable()
        {
            var view = new DependencyObject();
            var backupView = new DependencyObject();
            var cmd = new CommandAction(view, backupView, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);

            View.SetActionTarget(backupView, this.target);
            view.SetValue(FrameworkElement.DataContextProperty, this.target);

            cmd.Execute(null);
            Assert.IsTrue(this.target.DoSomethingCalled);
        }
    }
}
