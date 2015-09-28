using System;

namespace StyletIoC.Creation
{
    public class BuilderTypeKey : IEquatable<BuilderTypeKey>
    {
        public Type Type { get; set; }
        public string Key { get; set; }

        public BuilderTypeKey(Type type)
        {
            this.Type = type;
        }

        public BuilderTypeKey(Type type, string key)
        {
            this.Type = type;
            this.Key = key;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as BuilderTypeKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.Type.GetHashCode();
                if (this.Key != null)
                    hash = hash * 23 + this.Key.GetHashCode();
                return hash;
            }
        }

        public bool Equals(BuilderTypeKey other)
        {
            return other != null &&
                this.Type == other.Type &&
                other.Key == this.Key;
        }
    }
}
