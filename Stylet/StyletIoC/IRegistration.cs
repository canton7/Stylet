using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StyletIoC
{
    /// <summary>
    /// Delegate used to create an IRegistration
    /// </summary>
    /// <param name="creator">ICreator used by the IRegistration to create new instances</param>
    /// <param name="key">Key associated with the registration</param>
    /// <returns>A new IRegistration</returns>
    public delegate IRegistration RegistrationFactory(IRegistrationContext parentContext, ICreator creator, string key);

    /// <summary>
    /// An IRegistration is responsible to returning an appropriate (new or cached) instanced of a type, or an expression doing the same.
    /// It owns an ICreator, and will use it to create a new instance when needed.
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// Type of the object returned by the registration
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Fetches an instance of the relevaent type
        /// </summary>
        /// <returns>An object of type Type, which is supplied by the ICreator</returns>
        Func<IRegistrationContext, object> GetGenerator();

        /// <summary>
        /// Fetches an expression which evaluates to an instance of the relevant type
        /// </summary>
        /// <returns>An expression evaluating to an instance of type Type, which is supplied by the ICreator></returns>
        Expression GetInstanceExpression(ParameterExpression registrationContext);

        IRegistration CloneToContext(IRegistrationContext context);
    }

    internal abstract class RegistrationBase : IRegistration
    {
        protected readonly ICreator creator;
        public Type Type { get { return this.creator.Type; } }

        private readonly object generatorLock = new object();
        protected Func<IRegistrationContext, object> generator;

        public RegistrationBase(ICreator creator)
        {
            this.creator = creator;
        }

        public virtual Func<IRegistrationContext, object> GetGenerator()
        {
            if (this.generator != null)
                return this.generator;

            lock (this.generatorLock)
            {
                if (this.generator == null)
                {
                    this.generator = this.GetGeneratorInternal();
                }
                return this.generator;
            }
        }

        protected virtual Func<IRegistrationContext, object> GetGeneratorInternal()
        {
            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            return Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);

        public virtual IRegistration CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }

    internal class TransientRegistration : RegistrationBase
    {
        public TransientRegistration(ICreator creator) : base(creator) { }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return this.creator.GetInstanceExpression(registrationContext);
        }
    }

    internal class SingletonRegistration : RegistrationBase
    {
        private Expression instanceExpression;
        private object instance;
        private readonly IRegistrationContext parentContext;
        private bool disposed = false;

        public SingletonRegistration(IRegistrationContext parentContext, ICreator creator) : base(creator)
        {
            this.parentContext = parentContext;
            this.parentContext.Disposing += (o, e) =>
            {
                this.disposed = true;

                var disposable = this.instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                this.instance = null;
                this.instanceExpression = null;
                this.generator = null;
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.disposed)
                throw new ObjectDisposedException(String.Format("Singleton registration for type {0}", this.Type.Description()));

            if (this.instanceExpression != null)
                return this.instanceExpression;

            this.instance = Expression.Lambda<Func<IRegistrationContext, object>>(this.creator.GetInstanceExpression(registrationContext), registrationContext).Compile()(this.parentContext);

            // This expression yields the actual type of instance, not 'object'
            var instanceExpression = Expression.Constant(this.instance);
            Interlocked.CompareExchange(ref this.instanceExpression, instanceExpression, null);
            return this.instanceExpression;
        }
    }

    internal class PerContainerRegistration : RegistrationBase
    {
        private readonly IRegistrationContext parentContext;
        private readonly string key;
        private readonly object instanceFactoryLock = new object();
        private Func<IRegistrationContext, object> instanceFactory;
        private object instance;
        private bool disposed = false;

        private static readonly MethodInfo getMethod = typeof(IContainer).GetMethod("Get", new[] { typeof(Type), typeof(string) });

        public PerContainerRegistration(IRegistrationContext parentContext, ICreator creator, string key, Func<IRegistrationContext, object> instanceFactory = null)
            : base(creator)
        {
            this.parentContext = parentContext;
            this.key = key;
            this.instanceFactory = instanceFactory;

            this.parentContext.Disposing += (o, e) =>
            {
                this.disposed = true;

                var disposable = this.instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                this.instance = null;
                this.generator = null;
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
                if (this.disposed)
                    throw new ObjectDisposedException(String.Format("ChildContainer registration for type {0}", this.Type.Description()));

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

    // We're only created when we're needed, so no point in trying to be lazy
    internal class FuncNoKeyRegistration : IRegistration
    {
        private IRegistration delegateRegistration;
        private readonly Type funcType;
        private readonly Func<IRegistrationContext, object> generator;

        public Type Type
        {
            get { return this.funcType; }
        }

        public FuncNoKeyRegistration(IRegistration delegateRegistration)
        {
            this.delegateRegistration = delegateRegistration;
            this.funcType = Expression.GetFuncType(this.delegateRegistration.Type);

            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            return this.generator;
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return Expression.Lambda(this.delegateRegistration.GetInstanceExpression(registrationContext));
        }

        public IRegistration CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }

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

    internal class GetAllRegistration : IRegistration
    {
        private readonly IRegistrationContext parentContext;

        public string Key { get; private set; }
        private readonly Type _type;
        public Type Type
        {
            get { return this._type; }
        }

        private Expression expression;
        private readonly object generatorLock = new object();
        private Func<IRegistrationContext, object> generator;

        public GetAllRegistration(Type type, IRegistrationContext parentContext, string key)
        {
            this.Key = key;
            this._type = type;
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

            var list = Expression.New(this.Type);
            var init = Expression.ListInit(list, this.parentContext.GetRegistrations(new TypeKey(this.Type.GenericTypeArguments[0], this.Key), false).GetAll().Select(x => x.GetInstanceExpression(registrationContext)));

            Interlocked.CompareExchange(ref this.expression, init, null);
            return this.expression;
        }

        public IRegistration CloneToContext(IRegistrationContext context)
        {
            throw new InvalidOperationException("should not be cloned");
        }
    }
}
