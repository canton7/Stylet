using System;

namespace StyletIoC.Creation
{
    public class BuilderTypeKey
    {
        public Type Type { get; set; }
        public string Key { get; set; }

        public BuilderTypeKey(Type type)
        {
            this.Type = type;
        }
    }
}
