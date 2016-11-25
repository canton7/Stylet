using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Base for ViewModels which require property validation
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "According to Albahari and Albahari, relying on the GC to tidy up WaitHandles is arguably acceptable, since they're so small.")]
    public class ValidatingModelBase : PropertyChangedBase, INotifyDataErrorInfo
    {
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly SemaphoreSlim propertyErrorsLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, string[]> propertyErrors = new Dictionary<string, string[]>();
        private IModelValidator _validator;

        /// <summary>
        /// Gets or sets the IModelValidator to use to validate properties. You're expected to write your own, using your favourite validation library
        /// </summary>
        protected virtual IModelValidator Validator
        {
            get { return this._validator; }
            set
            {
                this._validator = value;
                if (this._validator != null)
                    this._validator.Initialize(this);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to run validation for a property automatically every time that property changes
        /// </summary>
        protected bool AutoValidate { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidatingModelBase"/> class, without using an <see cref="IModelValidator"/>
        /// </summary>
        public ValidatingModelBase()
        {
            this.AutoValidate = true;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidatingModelBase"/> class, using the specifies <see cref="IModelValidator"/>
        /// </summary>
        /// <param name="validator">Validator adapter to use to perform validations</param>
        public ValidatingModelBase(IModelValidator validator) : this()
        {
            // Can't set this.validator, as it's virtual, and FxCop complains
            this._validator = validator;
            if (this._validator != null)
                this._validator.Initialize(this);
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
        /// Validate all properties, synchronously
        /// </summary>
        /// <returns>True if all properties validated successfully</returns>
        protected bool Validate()
        {
            try
            {
                return this.ValidateAsync().Result;
            }
            catch (AggregateException e)
            {
                // We're only ever going to get one InnerException here - let's be nice and unwrap it
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Validate all properties.
        /// </summary>
        /// <returns>True if all properties validated successfully</returns>
        /// <remarks>If you override this, you MUST fire ErrorsChanged as appropriate, and call ValidationStateChanged</remarks>
        protected virtual async Task<bool> ValidateAsync()
        {
            if (this.Validator == null)
                throw new InvalidOperationException("Can't run validation if a validator hasn't been set");

            // We need the ConfigureAwait(false), as we might be called synchronously
            // However this means that the stuff after the await can be run in parallel on multiple threads
            // Therefore, we need the lock
            // However, we can't raise PropertyChanged events from within the lock, otherwise deadlock
            var results = await this.Validator.ValidateAllPropertiesAsync().ConfigureAwait(false);
            if (results == null)
                results = new Dictionary<string, IEnumerable<string>>();

            var changedProperties = new List<string>();
            await this.propertyErrorsLock.WaitAsync().ConfigureAwait(false);
            try
            {
                foreach (var kvp in results)
                {
                    var newErrors = kvp.Value == null ? null : kvp.Value.ToArray();
                    if (!this.propertyErrors.ContainsKey(kvp.Key))
                        this.propertyErrors[kvp.Key] = newErrors;
                    else if (this.ErrorsEqual(this.propertyErrors[kvp.Key], newErrors))
                        continue;
                    else
                        this.propertyErrors[kvp.Key] = newErrors;
                    changedProperties.Add(kvp.Key);
                }

                // If they haven't included a key in their validation results, that counts as no validation error
                foreach (var removedKey in this.propertyErrors.Keys.Except(results.Keys).ToArray())
                {
                    this.propertyErrors[removedKey] = null;
                    changedProperties.Add(removedKey);
                }
            }
            finally
            {
                this.propertyErrorsLock.Release();
            }

            if (changedProperties.Count > 0)
                this.OnValidationStateChanged(changedProperties);

            return !this.HasErrors;
        }

        /// <summary>
        /// Record a property error (or clear an error on a property). You can use this independently of the validation done by <see cref="Validator"/>
        /// </summary>
        /// <param name="property">Name of the property to change the errors for (or <see cref="String.Empty"/> to change the errors for the whole model)</param>
        /// <param name="errors">The new errors, or null to clear errors for this property</param>
        protected virtual void RecordPropertyError<TProperty>(Expression<Func<TProperty>> property, string[] errors)
        {
            this.RecordPropertyError(property.NameForProperty(), errors);
        }

        /// <summary>
        /// Record a property error (or clear an error on a property). You can use this independently of the validation done by <see cref="Validator"/>
        /// </summary>
        /// <param name="propertyName">Name of the property to change the errors for (or <see cref="String.Empty"/> to change the errors for the whole model)</param>
        /// <param name="errors">The new errors, or null to clear errors for this property</param>
        protected virtual void RecordPropertyError(string propertyName, string[] errors)
        {
            if (propertyName == null)
                propertyName = String.Empty;

            bool changed = false;
            this.propertyErrorsLock.Wait();
            try
            {
                string[] existingErrors;
                if (!this.propertyErrors.TryGetValue(propertyName, out existingErrors) || !this.ErrorsEqual(errors, existingErrors))
                {
                    this.propertyErrors[propertyName] = errors;
                    changed = true;
                }
            }
            finally
            {
                this.propertyErrorsLock.Release();
            }

            if (changed)
            {
                this.OnValidationStateChanged(new[] { propertyName });
            }
        }

        /// <summary>
        /// Clear all property errors
        /// </summary>
        protected virtual void ClearAllPropertyErrors()
        {
            List<string> changedProperties;

            this.propertyErrorsLock.Wait();
            try
            {
                changedProperties = this.propertyErrors.Keys.ToList();
                this.propertyErrors.Clear();
            }
            finally
            {
                this.propertyErrorsLock.Release();
            }

            if (changedProperties.Count > 0)
            {
                this.OnValidationStateChanged(changedProperties);
            }
        }

        /// <summary>
        /// Validate a single property synchronously, by name
        /// </summary>
        /// <typeparam name="TProperty">Type of property to validate</typeparam>
        /// <param name="property">Expression describing the property to validate</param>
        /// <returns>True if the property validated successfully</returns>
        protected virtual bool ValidateProperty<TProperty>(Expression<Func<TProperty>> property)
        {
            return this.ValidateProperty(property.NameForProperty());
        }

        /// <summary>
        /// Validate a single property asynchronously, by name
        /// </summary>
        /// <typeparam name="TProperty">Type ofproperty to validate</typeparam>
        /// <param name="property">Expression describing the property to validate</param>
        /// <returns>True if the property validated successfully</returns>
        protected virtual Task<bool> ValidatePropertyAsync<TProperty>(Expression<Func<TProperty>> property)
        {
            return this.ValidatePropertyAsync(property.NameForProperty());
        }

        /// <summary>
        /// Validate a single property synchronously, by name.
        /// </summary>
        /// <param name="propertyName">Property to validate</param>
        /// <returns>True if the property validated successfully</returns>
        protected bool ValidateProperty([CallerMemberName] string propertyName = null)
        {
            try
            {
                return this.ValidatePropertyAsync(propertyName).Result;
            }
            catch (AggregateException e)
            {
                // We're only ever going to get one InnerException here. Let's be nice and unwrap it
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Validate a single property asynchronously, by name.
        /// </summary>
        /// <param name="propertyName">Property to validate. Validates the entire model if null or <see cref="String.Empty"/></param>
        /// <returns>True if the property validated successfully</returns>
        /// <remarks>If you override this, you MUST fire ErrorsChanged and call OnValidationStateChanged() if appropriate</remarks>
        protected virtual async Task<bool> ValidatePropertyAsync([CallerMemberName] string propertyName = null)
        {
            if (this.Validator == null)
                throw new InvalidOperationException("Can't run validation if a validator hasn't been set");

            if (propertyName == null)
                propertyName = String.Empty;

            // To allow synchronous calling of this method, we need to resume on the ThreadPool.
            // Therefore, we might resume on any thread, hence the need for a lock
            var newErrorsRaw = await this.Validator.ValidatePropertyAsync(propertyName).ConfigureAwait(false);
            var newErrors = newErrorsRaw == null ? null : newErrorsRaw.ToArray();
            bool propertyErrorsChanged = false;

            await this.propertyErrorsLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!this.propertyErrors.ContainsKey(propertyName))
                    this.propertyErrors.Add(propertyName, null);

                if (!this.ErrorsEqual(this.propertyErrors[propertyName], newErrors))
                {
                    this.propertyErrors[propertyName] = newErrors;
                    propertyErrorsChanged = true;
                }
            }
            finally
            {
                this.propertyErrorsLock.Release();
            }

            if (propertyErrorsChanged)
                this.OnValidationStateChanged(new[] { propertyName });

            return newErrors == null || newErrors.Length == 0;
        }

        /// <summary>
        /// Raise a PropertyChanged notification for the named property, and validate that property if this.validation is set and this.autoValidate is true
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override async void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            // Save ourselves a little bit of work every time HasErrors is fired as the result of 
            // the validation results changing.
            if (this.Validator != null && this.AutoValidate && propertyName != "HasErrors")
                await this.ValidatePropertyAsync(propertyName);
        }

        /// <summary>
        /// Called whenever the error state of any properties changes. Calls NotifyOfPropertyChange("HasErrors") by default
        /// </summary>
        /// <param name="changedProperties">List of property names which have changed validation state</param>
        protected virtual void OnValidationStateChanged(IEnumerable<string> changedProperties)
        {
            this.NotifyOfPropertyChange("HasErrors");
            foreach (var property in changedProperties)
            {
                this.RaiseErrorsChanged(property);
            }
        }

        /// <summary>
        /// Raise the ErrorsChanged event for a given property
        /// </summary>
        /// <param name="propertyName">Property to raise the ErrorsChanged event for</param>
        protected virtual void RaiseErrorsChanged(string propertyName)
        {
            var handler = this.ErrorsChanged;
            if (handler != null)
                this.PropertyChangedDispatcher(() => handler(this, new DataErrorsChangedEventArgs(propertyName)));
        }

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or System.String.Empty, to retrieve entity-level errors.</param>
        /// <returns>The validation errors for the property or entity.</returns>
        public virtual IEnumerable GetErrors(string propertyName)
        {
            string[] errors;

            if (propertyName == null)
                propertyName = String.Empty;

            // We'll just have to wait synchronously for this. Oh well. The lock shouldn't be long.
            // Everything that awaits uses ConfigureAwait(false), so we shouldn't deadlock if someone calls this on the main thread
            this.propertyErrorsLock.Wait();
            try
            {
                this.propertyErrors.TryGetValue(propertyName, out errors);
            }
            finally
            {
                this.propertyErrorsLock.Release();
            }
            
            return errors;
        }

        /// <summary>
        /// Gets a value indicating whether the entity has validation errors.
        /// </summary>
        public virtual bool HasErrors
        {
            get { return this.propertyErrors.Values.Any(x => x != null && x.Length > 0); }
        }
    }
}
