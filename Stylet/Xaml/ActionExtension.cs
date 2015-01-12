using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

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
        /// Throw an exception
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
        public string Method { get; set; }

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
        /// <param name="method">Name of the method to call</param>
        public ActionExtension(string method)
        {
            this.Method = method;
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            // Seems this is the case when we're in a template. We'll get called again properly in a second.
            // http://social.msdn.microsoft.com/Forums/vstudio/en-US/a9ead3d5-a4e4-4f9c-b507-b7a7d530c6a9/gaining-access-to-target-object-instead-of-shareddp-in-custom-markupextensions-providevalue-method?forum=wpf
            if (!(valueService.TargetObject is DependencyObject))
                return this;

            var propertyAsDependencyProperty = valueService.TargetProperty as DependencyProperty;
            if (propertyAsDependencyProperty != null && propertyAsDependencyProperty.PropertyType == typeof(ICommand))
            {
                // If they're in design mode and haven't set View.ActionTarget, default to looking sensible
                var nullTarget = this.NullTarget == ActionUnavailableBehaviour.Default ? (Execute.InDesignMode ? ActionUnavailableBehaviour.Enable : ActionUnavailableBehaviour.Disable) : this.NullTarget;
                var actionNotFound = this.ActionNotFound == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Throw : this.ActionNotFound;
                return new CommandAction((DependencyObject)valueService.TargetObject, this.Method, nullTarget, actionNotFound);
            }

            if (valueService.TargetProperty is EventInfo)
            {
                var nullTarget = this.NullTarget == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Enable : this.NullTarget;
                var actionNotFound = this.ActionNotFound == ActionUnavailableBehaviour.Default ? ActionUnavailableBehaviour.Throw : this.ActionNotFound;
                var ec = new EventAction((DependencyObject)valueService.TargetObject, this.Method, nullTarget, actionNotFound);
                return ec.GetDelegate();
            }
                
            throw new ArgumentException("Can only use ActionExtension with a Command property or an event handler");
        }
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
