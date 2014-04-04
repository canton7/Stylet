using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public class LabelledValue<T> : IEquatable<LabelledValue<T>>
    {
        public string Label { get; set; }
        public T Value { get; set; }

        public LabelledValue(string label, T value)
        {
            this.Label = label;
            this.Value = value;
        }

        public bool Equals(LabelledValue<T> other)
        {
            return this.Label == other.Label && EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return (obj is LabelledValue<T>) ? this.Equals((LabelledValue<T>)obj) : false;
        }

        public override int GetHashCode()
        {
            return this.Label.GetHashCode() ^ this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Label;
        }
    }

    public static class LabelledValue
    {
        public static LabelledValue<T> Create<T>(string label, T value)
        {
            return new LabelledValue<T>(label, value);
        }
    }
}
