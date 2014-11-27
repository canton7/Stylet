using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
