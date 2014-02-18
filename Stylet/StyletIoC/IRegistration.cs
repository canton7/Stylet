using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletIoC
{
    internal interface IRegistration
    {
        Type Type { get; }
        bool WasAutoCreated { get; set; }
        Func<object> GetGenerator();
        Expression GetInstanceExpression();
    }

    internal abstract class RegistrationBase : IRegistration
    {
        protected ICreator creator;

        public Type Type { get { return this.creator.Type; } }
        public bool WasAutoCreated { get; set; }

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
            if (this.generator == null)
                this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression()).Compile();
            return this.generator;
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
            Interlocked.CompareExchange(ref this.instance, Expression.Lambda<Func<object>>(this.creator.GetInstanceExpression()).Compile()(), null);
        }

        public override Func<object> GetGenerator()
        {
            this.EnsureInstantiated();

            if (this.generator == null)
                this.generator = () => this.instance;

            return this.generator;
        }

        public override Expression GetInstanceExpression()
        {
            if (this.instanceExpression != null)
                return this.instanceExpression;

            this.EnsureInstantiated();

            // This expression yields the actual type of instance, not 'object'
            this.instanceExpression = Expression.Constant(this.instance);
            return this.instanceExpression;
        }
    }

    internal class GetAllRegistration : IRegistration
    {
        private StyletIoCContainer container;

        public string Key { get; set; }
        public Type Type { get; private set; }
        public bool WasAutoCreated { get; set; }

        private Expression expression;
        private Func<object> generator;

        public GetAllRegistration(Type type, StyletIoCContainer container)
        {
            this.Type = type;
            this.container = container;
        }

        public Func<object> GetGenerator()
        {
            if (this.generator == null)
                this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression()).Compile();
            return this.generator;
        }

        public Expression GetInstanceExpression()
        {
            if (this.expression != null)
                return this.expression;

            var list = Expression.New(this.Type);
            var init = Expression.ListInit(list, this.container.GetRegistrations(new TypeKey(this.Type.GenericTypeArguments[0], this.Key), false).GetAll().Select(x => x.GetInstanceExpression()));

            this.expression = init;
            return this.expression;
        }
    }
}
