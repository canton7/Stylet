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
    public delegate IRegistration RegistrationFactory(ICreator creator, string key);

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
        Func<object> GetGenerator();

        /// <summary>
        /// Fetches an expression which evaluates to an instance of the relevant type
        /// </summary>
        /// <returns>An expression evaluating to an instance of type Type, which is supplied by the ICreator></returns>
        Expression GetInstanceExpression();
    }

    internal abstract class RegistrationBase : IRegistration
    {
        protected readonly ICreator creator;
        public Type Type { get { return this.creator.Type; } }
        protected readonly object generatorLock = new object();
        // Value type, so needs locked access
        protected Func<object> generator { get; set; }

        public RegistrationBase(ICreator creator)
        {
            this.creator = creator;
        }

        public abstract Func<object> GetGenerator();
        public abstract Expression GetInstanceExpression();
    }


    internal class TransientRegistration : RegistrationBase
    {
        public TransientRegistration(ICreator creator) : base(creator) { }

        public override Expression GetInstanceExpression()
        {
            return this.creator.GetInstanceExpression();
        }

        public override Func<object> GetGenerator()
        {
            // Compiling the generator might be expensive, but there's nothing to be gained from
            // doing it outside of the lock - the altnerative is having two threads compiling it in parallel,
            // while would take just as long and use more resources
            lock (this.generatorLock)
            {
                if (this.generator != null)
                    return this.generator;
                var generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression()).Compile();
                if (this.generator == null)
                    this.generator = generator;
                return this.generator;
            }
        }
    }

    internal class SingletonRegistration : RegistrationBase
    {
        private object instance;
        private Expression instanceExpression;

        public SingletonRegistration(ICreator creator) : base(creator) { }

        private void EnsureInstantiated()
        {
            if (this.instance != null)
                return;

            // Ensure we don't end up creating two singletons, one used by each thread
            var instance = Expression.Lambda<Func<object>>(this.creator.GetInstanceExpression()).Compile()();
            Interlocked.CompareExchange(ref this.instance, instance, null);
        }

        public override Func<object> GetGenerator()
        {
            this.EnsureInstantiated();

            // Cheap delegate creation, so doesn't need to be outside the lock
            lock (this.generatorLock)
            {
                if (this.generator == null)
                    this.generator = () => this.instance;
                return this.generator;
            }
        }

        public override Expression GetInstanceExpression()
        {
            if (this.instanceExpression != null)
                return this.instanceExpression;

            this.EnsureInstantiated();

            // This expression yields the actual type of instance, not 'object'
            var instanceExpression = Expression.Constant(this.instance);
            Interlocked.CompareExchange(ref this.instanceExpression, instanceExpression, null);
            return this.instanceExpression;
        }
    }

    internal class GetAllRegistration : IRegistration
    {
        private readonly StyletIoCContainer container;

        public string Key { get; set; }
        private readonly Type _type;
        public Type Type
        {
            get { return this._type; }
        }

        private Expression expression;
        private readonly object generatorLock = new object();
        private Func<object> generator;

        public GetAllRegistration(Type type, StyletIoCContainer container)
        {
            this._type = type;
            this.container = container;
        }

        public Func<object> GetGenerator()
        {
            lock (this.generatorLock)
            {
                if (this.generator == null)
                    this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression()).Compile();
                return this.generator;
            }
        }

        public Expression GetInstanceExpression()
        {
            if (this.expression != null)
                return this.expression;

            var list = Expression.New(this.Type);
            var init = Expression.ListInit(list, this.container.GetRegistrations(new TypeKey(this.Type.GenericTypeArguments[0], this.Key), false).GetAll().Select(x => x.GetInstanceExpression()));

            Interlocked.CompareExchange(ref this.expression, init, null);
            return this.expression;
        }
    }
}
