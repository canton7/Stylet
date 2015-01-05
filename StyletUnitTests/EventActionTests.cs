using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class EventActionTests
    {
        private class Target
        {
            public bool DoSomethingCalled;
            public void DoSomething()
            {
                this.DoSomethingCalled = true;
            }

            public void DoSomethingWithTooManyArgs(object arg1, object arg2)
            {
            }

            public void DoSomethingWithBadArgument(string arg)
            {
            }

            public RoutedEventArgs EventArgs;
            public void DoSomethingWithEventArgs(RoutedEventArgs ea)
            {
                this.EventArgs = ea;
            }
        }

        private class Target2
        {
        }

        private DependencyObject subject;
        private Target target;
        private EventInfo eventInfo;

        [SetUp]
        public void SetUp()
        {
            this.target = new Target();
            this.subject = new Button();
            this.eventInfo = typeof(Button).GetEvent("Click");
            View.SetActionTarget(this.subject, this.target);
        }

        [Test]
        public void ThrowsIfNullTargetBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfNonExistentActionBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Disable));
        }

        [Test]
        public void ThrowsIfTargetNullBehaviourIsThrowAndTargetBecomesNull()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Enable);
            Assert.Throws<ActionTargetNullException>(() => View.SetActionTarget(this.subject, null));
        }

        [Test]
        public void ThrowsIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Throw);
            Assert.Throws<ActionNotFoundException>(() => View.SetActionTarget(this.subject, new Target2()));
        }

        [Test]
        public void ThrowsIfMethodHasTooManyArguments()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithTooManyArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasBadParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithBadArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void InvokingCommandDoesNothingIfTargetIsNull()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandDoesNothingIfActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, new Target2());
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandCallsMethod()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            cmd.GetDelegate().DynamicInvoke(null, null);
            Assert.True(this.target.DoSomethingCalled);
        }

        [Test]
        public void InvokingCommandCallsMethodWithEventArgs()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomethingWithEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = new RoutedEventArgs();
            cmd.GetDelegate().DynamicInvoke(null, arg);
            Assert.AreEqual(arg, this.target.EventArgs);
        }
    }
}
