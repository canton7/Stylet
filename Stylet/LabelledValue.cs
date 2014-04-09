using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Key-value pair useful for attaching labels to objects and displaying them in the view
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class LabelledValue<T> : IEquatable<LabelledValue<T>>
    {
        /// <summary>
        /// Label associated with this item. This is displayed in your View
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Value associated with this item. This is used by your ViewModel
        /// </summary>
        public T Value { get; set; }

        public LabelledValue(string label, T value)
        {
            this.Label = label;
            this.Value = value;
        }

        public bool Equals(LabelledValue<T> other)
        {
            return other == null ? false : this.Label == other.Label && EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LabelledValue<T>);
        }

        public override int GetHashCode()
        {
            return new { this.Label, this.Value }.GetHashCode();
        }

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
        public static LabelledValue<T> Create<T>(string label, T value)
        {
            return new LabelledValue<T>(label, value);
        }
    }
}
