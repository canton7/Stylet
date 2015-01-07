using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Stylet.Samples.ModelValidation
{
    /// <summary>
    /// Type which can be converter to and from both a T and a string. Useful for binding a TextBox to, say, an int.
    /// </summary>
    /// <remarks>
    /// Stringable{T} has two value properties - StringValue, which is always set, and Value, which is set if Value can be converted to a T.
    /// If you create a new Stringable{T} from a T, both Value and StringValue are set. If you Create one from a string (using Stringable{T}.FromString)
    /// then StringValue is set, and Value is set if that string value can be converted to a T>
    /// IsValid indicates whether Value could be set.
    /// 
    /// This type has an associated TypeConverter, StringableConverter, which will be used by WPF to convert this Stringable{T} to and from a string. 
    /// </remarks>
    // If this is a struct, we avoid null issues
    [TypeConverter(typeof(StringableConverter))]
    public struct Stringable<T> : IEquatable<Stringable<T>>
    {
        private readonly string _stringValue;

        /// <summary>
        /// String representation of the value
        /// </summary>
        public string StringValue
        {
            get { return this._stringValue; }
        }

        private readonly T _value;
        /// <summary>
        /// Actual value, or default(T) if IsValid is false
        /// </summary>
        public T Value
        {
            get { return this._value; }
        }

        private readonly bool _isValid;
        /// <summary>
        /// True if Value ias a proper value (i.e. we were constructed from a T, or we were constructed from a string which could be converted to a T)
        /// </summary>
        public bool IsValid
        {
            get { return this._isValid; }
        }

        /// <summary>
        /// Create a new instance, representing the given value
        /// </summary>
        /// <param name="value">Value to represent</param>
        public Stringable(T value) : this(value, value.ToString(), true) { }

        private Stringable(T value, string stringValue, bool isValid)
        {
            this._value = value;
            this._stringValue = stringValue;
            this._isValid = isValid;
        }

        /// <summary>
        /// Create a new instance from the given string. If the string can be converted to a T, then IsValue is true and Value contains the converted value.
        /// If not, IsValid is false and Value is default(T)
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static Stringable<T> FromString(string stringValue)
        {
            T dest = default(T);
            bool isValid = false;
            
            // The TypeConverter for String can't convert it into anything else, so don't bother getting that
            var fromConverter = TypeDescriptor.GetConverter(typeof(T));
            if (fromConverter.CanConvertFrom(typeof(string)) && fromConverter.IsValid(stringValue))
            {
                dest = (T)fromConverter.ConvertFrom(stringValue);
                isValid = true;
            }

            return new Stringable<T>(dest, stringValue, isValid);
        }

        public static implicit operator T(Stringable<T> stringable)
        {
            return stringable.Value;
        }

        public static implicit operator Stringable<T>(T value)
        {
            return new Stringable<T>(value);
        }

        public override string ToString()
        {
            return this.StringValue;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Stringable<T>))
                return false;
            return base.Equals((Stringable<T>)obj);
        }

        public bool Equals(Stringable<T> other)
        {
            return EqualityComparer<T>.Default.Equals(this.Value, other.Value) && this.StringValue == other.StringValue;
        }

        public static bool operator ==(Stringable<T> o1, Stringable<T> o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(Stringable<T> o1, Stringable<T> o2)
        {
            return !(o1 == o2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 27 + this.Value.GetHashCode();
                hash = hash * 27 + this.StringValue.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// TypeConverter for Stringable{T}
    /// </summary>
    /// <remarks>
    /// This is used by WPF. This means that if a Stringable{T} property is bound to a string control (e.g. TextBox), then this TypeConverter
    /// is used to convert from that string back to a Stringable{T}
    /// </remarks>
    public class StringableConverter : TypeConverter
    {
        private readonly Type valueType;
        private readonly Func<string, object> generator;

        public StringableConverter(Type type)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Stringable<>) || type.GetGenericArguments().Length != 1)
                throw new ArgumentException("Incompatible type", "type");

            this.valueType = type;

            // Generate a Func<string, object> which gives us a Stringable<T>, given a string
            // WPF instantiates us once, then uses us lots, so the overhead of doing this here is worth it
            var fromMethod = type.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public);
            var param = Expression.Parameter(typeof(string));
            this.generator = Expression.Lambda<Func<string, object>>(Expression.TypeAs(Expression.Call(fromMethod, param), typeof(object)), param).Compile();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return this.generator(value as string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || destinationType == this.valueType || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                return String.Empty;
            // Common case = just call the overloaded ToString - no need for reflection
            if (destinationType == typeof(string))
                return value.ToString();
            var valueType = value.GetType();
            if (destinationType.IsAssignableFrom(this.valueType) && typeof(Stringable<>).IsAssignableFrom(valueType))
            {
                var valueProperty = valueType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                return valueProperty.GetValue(value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
