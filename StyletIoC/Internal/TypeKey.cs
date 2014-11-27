using System;

namespace StyletIoC.Internal
{
    /// <summary>
    /// Type + key tuple, used as a dictionary key
    /// </summary>
    internal class TypeKey : IEquatable<TypeKey>
    {
        public readonly Type Type;
        public readonly string Key;

        public TypeKey(Type type, string key)
        {
            this.Type = type;
            this.Key = key;
        }

        public override int GetHashCode()
        {
            if (this.Key == null)
                return this.Type.GetHashCode();
            return this.Type.GetHashCode() ^ this.Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TypeKey);
        }

        public bool Equals(TypeKey other)
        {
            return other != null && this.Type == other.Type && this.Key == other.Key;
        }
    }
}
