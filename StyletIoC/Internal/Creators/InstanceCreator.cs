using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Creators
{
    internal class InstanceCreator : ICreator
    {
        public Type Type { get; private set; }
        private readonly Expression instanceExpression;

        public InstanceCreator(object instance)
        {
            this.Type = instance.GetType();
            this.instanceExpression = Expression.Constant(instance, this.Type);
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return this.instanceExpression;
        }
    }
}
