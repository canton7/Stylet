using System;

namespace StyletIoC.Internal
{
    /// <summary>
    /// Type + key tuple, used as a dictionary key
    /// </summary>
    internal class TypeKey : IEquatable<TypeKey>
    {
        public readonly RuntimeTypeHandle TypeHandle;
        public readonly string Key;

        public TypeKey(RuntimeTypeHandle typeHandle, string key)
        {
            this.TypeHandle = typeHandle;
            this.Key = key;
        }

        public override int GetHashCode()
        {
            if (this.Key == null)
                return this.TypeHandle.GetHashCode();
            return this.TypeHandle.GetHashCode() ^ this.Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TypeKey);
        }

        public bool Equals(TypeKey other)
        {
            return other != null && this.TypeHandle.Equals(other.TypeHandle) && this.Key == other.Key;
        }
    }
}
