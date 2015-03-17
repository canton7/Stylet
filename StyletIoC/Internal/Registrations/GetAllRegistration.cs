using StyletIoC.Creation;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace StyletIoC.Internal.Registrations
{
    using System.Diagnostics;

    /// <summary>
    /// Knows how to generate an IEnumerable{T}, which contains all implementations of T
    /// </summary>
    internal class GetAllRegistration : IRegistration
    {
        private readonly IRegistrationContext parentContext;

        public string Key { get; private set; }
        private readonly RuntimeTypeHandle _type;
        public RuntimeTypeHandle TypeHandle
        {
            get { return this._type; }
        }

        private Expression expression;
        private readonly object generatorLock = new object();
        private Func<IRegistrationContext, object> generator;

        public GetAllRegistration(RuntimeTypeHandle typeHandle, IRegistrationContext parentContext, string key)
        {
            this.Key = key;
            this._type = typeHandle;
            this.parentContext = parentContext;
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            if (this.generator != null)
                return this.generator;

            lock (this.generatorLock)
            {
                if (this.generator == null)
                {
                    var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
                    this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
                }
                return this.generator;
            }
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.expression != null)
                return this.expression;

            var type = Type.GetTypeFromHandle(this.TypeHandle);

            var instanceExpressions = this.parentContext.GetAllRegistrations(type.GenericTypeArguments[0], this.Key, false).Select(x => x.GetInstanceExpression(registrationContext)).ToArray();
            var listCtor = type.GetConstructor(new[] { typeof(int) }); // ctor which takes capacity
            Debug.Assert(listCtor != null);
            var listNew = Expression.New(listCtor, Expression.Constant(instanceExpressions.Length));
            Expression list = instanceExpressions.Any() ? (Expression)Expression.ListInit(listNew, instanceExpressions) : listNew;

            if (StyletIoCContainer.CacheGeneratedExpressions)
            {
                Interlocked.CompareExchange(ref this.expression, list, null);
                return this.expression;
            }
            else
            {
                return list;
            }
        }
    }
}
