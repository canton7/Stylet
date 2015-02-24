using Stylet.Logging;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Created by ActionExtension, this can return a delegate suitable adding binding to an event, and can call a method on the View.ActionTarget
    /// </summary>
    public class EventAction : ActionBase
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(EventAction));
        private static readonly MethodInfo invokeCommandMethodInfo = typeof(EventAction).GetMethod("InvokeCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Type of event handler
        /// </summary>
        private readonly Type eventHandlerType;

        /// <summary>
        /// Initialises a new instance of the <see cref="EventAction"/> class
        /// </summary>
        /// <param name="subject">View whose View.ActionTarget we watch</param>
        /// <param name="eventHandlerType">Type of event handler we're returning a delegate for</param>
        /// <param name="methodName">The MyMethod in {s:Action MyMethod}, this is what we call when the event's fired</param>
        /// <param name="targetNullBehaviour">Behaviour for it the relevant View.ActionTarget is null</param>
        /// <param name="actionNonExistentBehaviour">Behaviour for if the action doesn't exist on the View.ActionTarget</param>
        public EventAction(DependencyObject subject, Type eventHandlerType, string methodName, ActionUnavailableBehaviour targetNullBehaviour, ActionUnavailableBehaviour actionNonExistentBehaviour)
            : base(subject, methodName, targetNullBehaviour, actionNonExistentBehaviour, logger)
        {
            if (targetNullBehaviour == ActionUnavailableBehaviour.Disable)
                throw new ArgumentException("Setting NullTarget = Disable is unsupported when used on an Event");
            if (actionNonExistentBehaviour == ActionUnavailableBehaviour.Disable)
                throw new ArgumentException("Setting ActionNotFound = Disable is unsupported when used on an Event");

            this.eventHandlerType = eventHandlerType;
        }

        /// <summary>
        /// Invoked when a new non-null target is set, which has non-null MethodInfo. Used to assert that the method signature is correct
        /// </summary>
        /// <param name="targetMethodInfo">MethodInfo of method on new target</param>
        /// <param name="newTargetType">Type of new target</param>
        protected internal override void AssertTargetMethodInfo(MethodInfo targetMethodInfo, Type newTargetType)
        {
            var methodParameters = targetMethodInfo.GetParameters();
            if (!(methodParameters.Length == 0 ||
                (methodParameters.Length == 1 && typeof(EventArgs).IsAssignableFrom(methodParameters[0].ParameterType)) ||
                (methodParameters.Length == 2 && typeof(EventArgs).IsAssignableFrom(methodParameters[1].ParameterType))))
            {
                var e = new ActionSignatureInvalidException(String.Format("Method {0} on {1} must have the signatures void Method(), void Method(EventArgsOrSubClass e), or void Method(object sender, EventArgsOrSubClass e)", this.MethodName, newTargetType.Name));
                logger.Error(e);
                throw e;
            }
        }

        /// <summary>
        /// Return a delegate which can be added to the targetProperty
        /// </summary>
        /// <returns>An event hander, which, when invoked, will invoke the action</returns>
        public Delegate GetDelegate()
        {
            var del = Delegate.CreateDelegate(this.eventHandlerType, this, invokeCommandMethodInfo, false);
            if (del == null)
            {
                var e = new ActionEventSignatureInvalidException(String.Format("Event being bound to does not have the '(object sender, EventArgsOrSubclass e)' signature we were expecting. Method {0} on target {1}", this.MethodName, this.Target));
                logger.Error(e);
                throw e;
            }
            return del;
        }

        // ReSharper disable once UnusedMember.Local
        private void InvokeCommand(object sender, EventArgs e)
        {
            // If we've made it this far and the target is still the default, then something's wrong
            // Make sure they know
            if (this.Target == View.InitialActionTarget)
            {
                var ex = new ActionNotSetException(String.Format("View.ActionTarget not on control {0} (method {1}). " +
                    "This probably means the control hasn't inherited it from a parent, e.g. because a ContextMenu or Popup sits in the visual tree. " +
                    "You will need so set 's:View.ActionTarget' explicitly. See the wiki for more details.", this.Subject, this.MethodName));
                logger.Error(ex);
                throw ex;
            }

            // Any throwing will have been handled above
            if (this.Target == null || this.TargetMethodInfo == null)
                return;

            object[] parameters;
            switch (this.TargetMethodInfo.GetParameters().Length)
            {
                case 1:
                    parameters = new object[] { e };
                    break;
                    
                case 2:
                    parameters = new[] { sender, e };
                    break;

                default:
                    parameters = null;
                    break;
            }

            logger.Info("Invoking method {0} on target {1} with parameters ({2})", this.MethodName, this.Target, parameters == null ? "none" : String.Join(", ", parameters));

            try
            {
                this.TargetMethodInfo.Invoke(this.Target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                // Be nice and unwrap this for them
                // They want a stack track for their VM method, not us
                logger.Error(ex.InnerException, String.Format("Failed to invoke method {0} on target {1} with parameters ({2})", this.MethodName, this.Target, parameters == null ? "none" : String.Join(", ", parameters)));
                // http://stackoverflow.com/a/17091351/1086121
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }
    }

    /// <summary>
    /// You tried to use an EventAction with an event that doesn't follow the EventHandler signature
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionEventSignatureInvalidException : Exception
    {
        internal ActionEventSignatureInvalidException(string message) : base(message) { }
    }
}
