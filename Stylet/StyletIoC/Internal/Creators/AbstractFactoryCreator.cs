using StyletIoC.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Creators
{
    internal sealed class AbstractFactoryCreator : ICreator
    {
        private readonly Type abstractFactoryType;
        public Type Type
        {
            get { return this.abstractFactoryType; }
        }

        public AbstractFactoryCreator(Type abstractFactoryType)
        {
            this.abstractFactoryType = abstractFactoryType;
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            var ctor = this.abstractFactoryType.GetConstructor(new[] { typeof(IRegistrationContext) });
            var construction = Expression.New(ctor, registrationContext);
            return construction;
        }
    }
}
