using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Knows how to generate an instance which has per-container scope
    /// </summary>
    internal class PerContainerRegistration : RegistrationBase
    {
        private readonly IRegistrationContext parentContext;
        private readonly string key;
        private readonly object instanceFactoryLock = new object();
        private Func<IRegistrationContext, object> instanceFactory;
        private object instance;

        private static readonly MethodInfo getMethod = typeof(IContainer).GetMethod("Get", new[] { typeof(Type), typeof(string) });

        public PerContainerRegistration(IRegistrationContext parentContext, ICreator creator, string key, Func<IRegistrationContext, object> instanceFactory = null)
            : base(creator)
        {
            this.parentContext = parentContext;
            this.key = key;
            this.instanceFactory = instanceFactory;

            this.parentContext.Disposing += (o, e) =>
            {
                var disposable = this.instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                this.instance = null;
            };
        }

        private void EnsureInstanceFactoryCreated()
        {
            if (this.instanceFactory == null)
            {
                lock (this.instanceFactoryLock)
                {
                    var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
                    this.instanceFactory = Expression.Lambda<Func<IRegistrationContext, object>>(this.creator.GetInstanceExpression(registrationContext), registrationContext).Compile();
                }
            }
        }

        protected override Func<IRegistrationContext, object> GetGeneratorInternal()
        {
            // If the context is our parent context, then everything's fine and we can return our instance
            // If not, well, this should never happen. When we're cloned to the new context, we set ourselves up with the new parent
            return ctx =>
            {
                Debug.Assert(ctx == this.parentContext);

                if (this.instance != null)
                    return this.instance;

                this.EnsureInstanceFactoryCreated();

                var instance = this.instanceFactory(ctx);
                Interlocked.CompareExchange(ref this.instance, instance, null);
                return this.instance;
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            // Always synthesize into a method call onto the current context
            var call = Expression.Call(registrationContext, getMethod, Expression.Constant(this.Type), Expression.Constant(this.key, typeof(string)));
            var cast = Expression.Convert(call, this.Type);
            return cast;
        }

        public override IRegistration CloneToContext(IRegistrationContext context)
        {
            // Ensure the factory's created, and pass it down. This means the work of compiling the creation expression is done once, ever
            this.EnsureInstanceFactoryCreated();
            return new PerContainerRegistration(context, this.creator, this.key, this.instanceFactory);
        }
    }
}
