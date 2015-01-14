using System;

namespace StyletIoC
{
    /// <summary>
    /// Attribute which can be used to mark the constructor to use, properties to inject, which key to use to resolve an injected property, and others. See the docs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="InjectAttribute"/> class
        /// </summary>
        public InjectAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="InjectAttribute"/> class, which has the specified key
        /// </summary>
        /// <param name="key">Key to associate (meaning depends on context)</param>
        public InjectAttribute(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// Gets or sets the key to use to resolve the relevant dependency
        /// </summary>
        public string Key { get; set; }
    }
}
