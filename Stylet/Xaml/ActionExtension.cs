using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;

namespace Stylet.Xaml
{
    /// <summary>
    /// What to do if the given target is null, or if the given action doesn't exist on the target
    /// </summary>
    public enum ActionUnavailableBehaviour
    {
        /// <summary>
        /// The default behaviour. What this is depends on whether this applies to an action or target, and an event or ICommand
        /// </summary>
        Default,

        /// <summary>
        /// Enable the control anyway. Clicking/etc the control won't do anything
        /// </summary>
        Enable,

        /// <summary>
        /// Disable the control. This is only valid for commands, not events
        /// </summary>
        Disable,

        /// <summary>
        /// An exception will be thrown when the control is clicked
        /// </summary>
        Throw
    }

    /// <summary>
    /// MarkupExtension used for binding Commands and Events to methods on the View.ActionTarget
    /// </summary>
    public class ActionExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the name of the method to call
        /// </summary>
        [ConstructorArgument("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets a target to override that set with View.ActionTarget
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Gets or sets the behaviour if the View.ActionTarget is nulil
        /// </summary>
        public ActionUnavailableBehaviour NullTarget { get; set; }

        /// <summary>
        /// Gets or sets the behaviour if the action itself isn't found on the View.ActionTarget
        /// </summary>
        public ActionUnavailableBehaviour ActionNotFound { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ActionExtension"/> class
        /// </summary>
        public ActionExtension()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ActionExtension"/> class with the given method name
        /// </summary>
        /// <param name="method">Name of the method to call</param>
        public ActionExtension(string method)
        {
            this.Method = method;
        }

        private ActionUnavailableBehaviour CommandNullTargetBehaviour
        {
            get { return this.NullTarget == ActionUnavailableBehaviour.Default ? (Execute.InDesignMode ? ActionUnavailableBehaviour.Enable : ActionUnavailableBehaviour.Disable) : this.NullTarget; }
        }

        private ActionUnavailableBehaviour CommandActionNotFoundBehaviour
        {
            get { return this.ActionNotFound == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Throw : this.ActionNotFound; }
        }

        private ActionUnavailableBehaviour EventNullTargetBehaviour
        {
            get { return this.NullTarget == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Enable : this.NullTarget; }
        }

        private ActionUnavailableBehaviour EventActionNotFoundBehaviour
        {
            get { return this.ActionNotFound == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Throw : this.ActionNotFound; }
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Method == null)
                throw new InvalidOperationException("Method has not been set");

            var valueService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            switch (valueService.TargetObject)
            {
                case DependencyObject targetObject:
                    return this.HandleDependencyObject(serviceProvider, valueService, targetObject);
                case CommandBinding commandBinding:
                    return this.CreateEventAction(serviceProvider, null, ((EventInfo)valueService.TargetProperty).EventHandlerType, isCommandBinding: true);
                default:
                    // Seems this is the case when we're in a template. We'll get called again properly in a second.
                    // http://social.msdn.microsoft.com/Forums/vstudio/en-US/a9ead3d5-a4e4-4f9c-b507-b7a7d530c6a9/gaining-access-to-target-object-instead-of-shareddp-in-custom-markupextensions-providevalue-method?forum=wpf
                    return this;
            }
        }

        private object HandleDependencyObject(IServiceProvider serviceProvider, IProvideValueTarget valueService, DependencyObject targetObject)
        {
            switch (valueService.TargetProperty)
            {
                case DependencyProperty dependencyProperty when dependencyProperty.PropertyType == typeof(ICommand):
                    // If they're in design mode and haven't set View.ActionTarget, default to looking sensible
                    return this.CreateCommandAction(serviceProvider, targetObject);
                case EventInfo eventInfo:
                    return this.CreateEventAction(serviceProvider, targetObject, eventInfo.EventHandlerType);
                case MethodInfo methodInfo: // For attached events
                    {
                        var parameters = methodInfo.GetParameters();
                        if (parameters.Length == 2 && typeof(Delegate).IsAssignableFrom(parameters[1].ParameterType))
                        {
                            return this.CreateEventAction(serviceProvider, targetObject, parameters[1].ParameterType);
                        }
                        throw new ArgumentException("Action used with an attached event (or something similar) which didn't follow the normal pattern");
                    }
                default:
                    throw new ArgumentException("Can only use ActionExtension with a Command property or an event handler");
            }
        }

        private ICommand CreateCommandAction(IServiceProvider serviceProvider, DependencyObject targetObject)
        {
            if (this.Target == null)
            {
                var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
                var rootObject = rootObjectProvider?.RootObject as DependencyObject;
                return new CommandAction(targetObject, rootObject, this.Method, this.CommandNullTargetBehaviour, this.CommandActionNotFoundBehaviour);
            }
            else
            {
                return new CommandAction(this.Target, this.Method, this.CommandNullTargetBehaviour, this.CommandActionNotFoundBehaviour);
            }
        }

        private Delegate CreateEventAction(IServiceProvider serviceProvider, DependencyObject targetObject, Type eventType, bool isCommandBinding = false)
        {
            EventAction ec;
            if (this.Target == null)
            {
                var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
                var rootObject = rootObjectProvider?.RootObject as DependencyObject;
                if (isCommandBinding)
                {
                    if (rootObject == null)
                        throw new InvalidOperationException("Action may only be used with CommandBinding from a XAML view (unable to retrieve IRootObjectProvider.RootObject)");
                    ec = new EventAction(rootObject, null, eventType, this.Method, this.EventNullTargetBehaviour, this.EventActionNotFoundBehaviour);
                }
                else
                {
                    ec = new EventAction(targetObject, rootObject, eventType, this.Method, this.EventNullTargetBehaviour, this.EventActionNotFoundBehaviour);
                }
            }
            else
            {
                ec = new EventAction(this.Target, eventType, this.Method, this.EventNullTargetBehaviour, this.EventActionNotFoundBehaviour);
            }

            return ec.GetDelegate();
        }
    }

    /// <summary>
    /// The View.ActionTarget was not set. This probably means the item is in a ContextMenu/Popup
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionNotSetException : Exception
    {
        internal ActionNotSetException(string message) : base(message) { }
    }

    /// <summary>
    /// The Action Target was null, and shouldn't have been (NullTarget = Throw)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionTargetNullException : Exception
    {
        internal ActionTargetNullException(string message) : base(message) { }
    }

    /// <summary>
    /// The method specified could not be found on the Action Target
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionNotFoundException : Exception
    {
        internal ActionNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// The method specified does not have the correct signature
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionSignatureInvalidException : Exception
    {
        internal ActionSignatureInvalidException(string message) : base(message) { }
    }
}
