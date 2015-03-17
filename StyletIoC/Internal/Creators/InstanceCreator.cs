using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Creators
{
    internal class InstanceCreator : ICreator
    {
        public RuntimeTypeHandle TypeHandle { get; private set; }
        private readonly Expression instanceExpression;

        public InstanceCreator(object instance)
        {
            var type = instance.GetType();
            this.TypeHandle = type.TypeHandle;
            this.instanceExpression = Expression.Constant(instance, type);
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return this.instanceExpression;
        }
    }
}
