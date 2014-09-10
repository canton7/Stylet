using System;
using System.Linq;
using System.Linq.Expressions;
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
                    var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
                    this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
                }
                return this.generator;
            }
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
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

    internal class GetAllRegistration : IRegistration
    {
        private readonly IRegistrationContext parentContext;

        public string Key { get; set; }
        private readonly Type _type;
        public Type Type
        {
            get { return this._type; }
        }

        private Expression expression;
        private readonly object generatorLock = new object();
        private Func<IRegistrationContext, object> generator;

        public GetAllRegistration(Type type, IRegistrationContext parentContext)
        {
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
    }
}
