using System;
using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// Key-value pair useful for attaching labels to objects and displaying them in the view
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class LabelledValue<T> : PropertyChangedBase, IEquatable<LabelledValue<T>>
    {
        private string _label;

        /// <summary>
        /// Gets or sets the label associated with this item. This is displayed in your View
        /// </summary>
        public string Label
        {
            get { return this._label; }
            set { this.SetAndNotify(ref this._label, value); }
        }

        private T _value;

        /// <summary>
        /// Gets or sets the value associated with this item. This is used by your ViewModel
        /// </summary>
        public T Value
        {
            get { return this._value; }
            set { this.SetAndNotify(ref this._value, value); }
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="LabelledValue{T}"/> class, without setting Label or Value
        /// </summary>
        public LabelledValue()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="LabelledValue{T}"/> class, with the given label and value
        /// </summary>
        /// <param name="label">Label to use. This value is displayed in your view</param>
        /// <param name="value">Value to use. This is used by your ViewModel</param>
        public LabelledValue(string label, T value)
        {
            this._label = label;
            this._value = value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(LabelledValue<T> other)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (Object.ReferenceEquals(other, null))
                return false;

           return this.Label == other.Label && EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of any type
        /// </summary>
        /// <param name="obj">An object to compare with this object</param>
        /// <returns>true if the current object is of the same type as the other object, and equal to the other parameter; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as LabelledValue<T>);
        }

        /// <summary>
        /// Returns a hash code for the this object
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                if (this.Label != null)
                    hash = hash * 23 + this.Label.GetHashCode();
                if (this.Value != null)
                    hash = hash * 23 + this.Value.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Return the Label associated with this object
        /// </summary>
        /// <returns>The Label associated with this object</returns>
        public override string ToString()
        {
            return this.Label;
        }
    }

    /// <summary>
    /// Convenience class for constructing LabellelValue{T}'s
    /// </summary>
    public static class LabelledValue
    {
        /// <summary>
        /// Construct a new LabelledValue{T}, using method type inference
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="label">Label to assign</param>
        /// <param name="value">Value to assign</param>
        /// <returns>Constructed LabelledValue{T}</returns>
        public static LabelledValue<T> Create<T>(string label, T value)
        {
            return new LabelledValue<T>(label, value);
        }
    }
}
