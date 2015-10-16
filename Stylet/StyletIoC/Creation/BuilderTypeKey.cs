using System;

namespace StyletIoC.Creation
{
    /// <summary>
    /// Defines and type + key for a service, used in setting up bindings
    /// </summary>
    public class BuilderTypeKey : IEquatable<BuilderTypeKey>
    {
        /// <summary>
        /// Gets or sets the Type associated with this Type+Key
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the Key associated with this Type+Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BuilderTypeKey"/> class with the given type
        /// </summary>
        /// <param name="type">Type to associated with this Type+Key</param>
        public BuilderTypeKey(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="BuilderTypeKey"/> class with the given type and key
        /// </summary>
        /// <param name="type">Type to associated with this Type+Key</param>
        /// <param name="key">Key to associated with this Type+Key</param>
        public BuilderTypeKey(Type type, string key)
        {
            this.Type = type;
            this.Key = key;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as BuilderTypeKey);
        }

        /// <summary>
        /// Calculates a HashCode for the current object
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + this.Type.GetHashCode();
                if (this.Key != null)
                    hash = (hash * 23) + this.Key.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(BuilderTypeKey other)
        {
            return other != null &&
                this.Type == other.Type &&
                other.Key == this.Key;
        }
    }
}
