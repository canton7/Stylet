using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    internal class FuncWithKeyRegistration : IRegistration
    {
        private readonly Type resultType;
        private readonly Type funcType;
        private readonly Func<IRegistrationContext, object> generator;

        private static readonly MethodInfo getMethod = typeof(IContainer).GetMethod("GetTypeOrAll", new[] { typeof(Type), typeof(string) });

        public Type Type
        {
            get { return this.funcType; }
        }

        public FuncWithKeyRegistration(Type resultType)
        {
            this.resultType = resultType;

            this.funcType = Expression.GetFuncType(typeof(string), this.resultType);
            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            return this.generator;
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            var input = Expression.Parameter(typeof(string), "key");
            var call = Expression.Call(registrationContext, getMethod, Expression.Constant(this.resultType), input);
            return Expression.Lambda(Expression.Convert(call, this.resultType), input);
        }

        public IRegistration CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }
}
