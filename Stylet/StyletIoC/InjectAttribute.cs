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
        /// Create a new InjectAttribute
        /// </summary>
        public InjectAttribute()
        {
        }

        /// <summary>
        /// Create a new InjectAttribute, which has the specified key
        /// </summary>
        /// <param name="key"></param>
        public InjectAttribute(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// Key to use to resolve the relevant dependency
        /// </summary>
        public string Key { get; set; }
    }
}
