using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Base for ViewModels which require property validation
    /// </summary>
    public class ValidatingModelBase : PropertyChangedBase, INotifyDataErrorInfo
    {
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly Dictionary<string, string[]> propertyErrors = new Dictionary<string, string[]>();
        private IValidatorAdapter _validator;

        /// <summary>
        /// IValidationAdapter to use to validate properties. You're expected to write your own, using your favourite validation library
        /// </summary>
        protected IValidatorAdapter validator
        {
            get { return this._validator; }
            set
            {
                this._validator = value;
                this._validator.Initialize(this);
            }
        }

        /// <summary>
        /// Whether to run validation for a property automatically every time that property changes
        /// </summary>
        protected bool autoValidate { get; set; }

        public ValidatingModelBase()
        {
            this.autoValidate = true;
        }

        private bool ErrorsEqual(string[] e1, string[] e2)
        {
            if (e1 == null && e2 == null)
                return true;
            if (e1 == null || e2 == null)
                return false;
            return e1.SequenceEqual(e2);
        }

        /// <summary>
        /// Validate all properties. If you override this, you MUST fire ErrorsChanged as appropriate, and call ValidationStateChanged
        /// </summary>
        protected virtual async Task ValidateAsync()
        {
            if (this.validator == null)
                return;

            var handler = this.ErrorsChanged;
            bool anyChanged = false;

            foreach (var kvp in await this.validator.ValidateAllPropertiesAsync())
            {
                if (!this.propertyErrors.ContainsKey(kvp.Key))
                    this.propertyErrors.Add(kvp.Key, null);

                if (this.ErrorsEqual(this.propertyErrors[kvp.Key], kvp.Value))
                    continue;

                this.propertyErrors[kvp.Key] = kvp.Value;
                anyChanged = true;
                if (handler != null)
                    handler(this, new DataErrorsChangedEventArgs(kvp.Key));
            }

            if (anyChanged)
                this.OnValidationStateChanged();
        }

        /// <summary>
        /// Call ValidateProperty, deriving the name of the property in a type-safe manner
        /// </summary>
        /// <param name="property">Expression describing the property to validate</param>
        protected virtual Task ValidatePropertyAsync<TProperty>(Expression<Func<TProperty>> property)
        {
            return this.ValidatePropertyAsync(property.NameForProperty());
        }

        /// <summary>
        /// Validate a single property, by name. If you override this, you MUST fire ErrorsChange and call OnValidationStateChanged() if appropriate
        /// </summary>
        /// <param name="propertyName">Property to validate</param>
        protected virtual async Task ValidatePropertyAsync(string propertyName)
        {
            if (this.validator == null)
                return;

            if (!this.propertyErrors.ContainsKey(propertyName))
                this.propertyErrors.Add(propertyName, null);

            var newErrors = await this.validator.ValidatePropertyAsync(propertyName);
            if (!this.ErrorsEqual(this.propertyErrors[propertyName], newErrors))
            {
                this.propertyErrors[propertyName] = newErrors;

                var handler = this.ErrorsChanged;
                if (handler != null)
                    handler(this, new DataErrorsChangedEventArgs(propertyName));
                this.OnValidationStateChanged();
            }
        }

        protected override async void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            // Save ourselves a little bit of work every time HasErrors is fired as the result of 
            // the validation results changing.
            if (this.autoValidate && propertyName != "HasErrors")
                await this.ValidatePropertyAsync(propertyName);
        }

        /// <summary>
        /// Called whenever the error state of any properties changes. Calls NotifyOfPropertyChange(() => this.HasErrors) by default
        /// </summary>
        protected virtual void OnValidationStateChanged()
        {
            this.NotifyOfPropertyChange(() => this.HasErrors);
        }

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or System.String.Empty, to retrieve entity-level errors.</param>
        /// <returns>The validation errors for the property or entity.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (this.propertyErrors.ContainsKey(propertyName))
                return this.propertyErrors[propertyName];
            return null;
        }

        /// <summary>
        /// Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        public bool HasErrors
        {
            get { return this.propertyErrors.Values.Any(x => x != null && x.Length > 0); }
        }
    }
}
