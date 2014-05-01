using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Generic version of IValidationArapter. Provided for use with StyletIoC
    /// </summary>
    /// <remarks>
    /// Having a generic version allows you implement it using a generic ValidationAdapter (ValidationAdapter{T} : IValidationAdapter{T})
    /// then write a binding rule like this:
    /// builder.Bind(typeof(IValidationAdapter{})).ToAllImplementations()
    /// and request a new IValidationAdapter{MyViewModelType} in your ViewModel's constructor.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IValidatorAdapter<in T> : IValidatorAdapter
    {
    }

    /// <summary>
    /// Adapter used by ValidationModelBase to perform validation.
    /// </summary>
    /// <remarks>
    /// This should be specialised to the particular ValidationModelBase instance it's validating
    /// </remarks>
    public interface IValidatorAdapter
    {
        /// <summary>
        /// Called by ValidatingModelBase, which passes in an instance of itself.
        /// This allows the IValidationAdapter to specialize to validating that particular ValidatingModelBase instance
        /// </summary>
        /// <param name="subject"></param>
        void Initialize(object subject);

        /// <summary>
        /// Validate a single property by name, and return an array of validation errors for that property (or null if validation was successful)
        /// </summary>
        /// <param name="propertyName">Property to validate</param>
        /// <returns>Array of validation errors, or null if validation was successful</returns>
        Task<string[]> ValidatePropertyAsync(string propertyName);

        /// <summary>
        /// Validate all properties, and return the results for all properties
        /// </summary>
        /// <remarks>
        /// If a property validates successfully, you MUST return a null entry for it in the returned dictionary!
        /// </remarks>
        /// <returns>A dictionary of property name => array of validation errors (or null if that property validated successfully)</returns>
        Task<Dictionary<string, string[]>> ValidateAllPropertiesAsync();
    }
}
