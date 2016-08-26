using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Generic version of IValidationArapter. Provided for use with StyletIoC
    /// </summary>
    /// <remarks>
    /// Having a generic version allows you implement it using a generic ModelValidator (ModelValidator{T} : IModelValidator{T})
    /// then write a binding rule like this:
    /// builder.Bind(typeof(IModelValidator{})).ToAllImplementations()
    /// and request a new IModelValidator{MyViewModelType} in your ViewModel's constructor.
    /// </remarks>
    /// <typeparam name="T">Type of model being validated</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IModelValidator<in T> : IModelValidator
    {
    }

    /// <summary>
    /// Adapter used by ValidationModelBase to perform validation.
    /// </summary>
    /// <remarks>
    /// This should be specialised to the particular ValidationModelBase instance it's validating
    /// </remarks>
    public interface IModelValidator
    {
        /// <summary>
        /// Called by ValidatingModelBase, which passes in an instance of itself.
        /// This allows the IModelValidator to specialize to validating that particular ValidatingModelBase instance
        /// </summary>
        /// <param name="subject">Subject to initialize</param>
        void Initialize(object subject);

        /// <summary>
        /// Validate a single property by name, and return an array of validation errors for that property (or null if validation was successful)
        /// </summary>
        /// <param name="propertyName">Property to validate, or <see cref="String.Empty"/> to validate the entire model</param>
        /// <returns>Array of validation errors, or null / empty if validation was successful</returns>
        Task<IEnumerable<string>> ValidatePropertyAsync(string propertyName);

        /// <summary>
        /// Validate all properties, and return the results for all properties
        /// </summary>
        /// <remarks>
        /// Use a key of <see cref="String.Empty"/> to indicate validation errors for the entire model.
        /// 
        /// If a property validates successfully, you MUST return a null entry for it in the returned dictionary!
        /// </remarks>
        /// <returns>A dictionary of property name => array of validation errors (or null if that property validated successfully)</returns>
        Task<Dictionary<string, IEnumerable<string>>> ValidateAllPropertiesAsync();
    }
}
