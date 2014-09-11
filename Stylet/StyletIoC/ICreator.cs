using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StyletIoC
{
    /// <summary>
    /// An ICreator is responsible for creating an instance of an object on demand
    /// </summary>
    public interface ICreator
    {
        /// <summary>
        /// Type of object that will be created
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Fetches an expression evaluating to an instance on demand
        /// </summary>
        /// <returns>An expression evaluating to an instance of the specified Type</returns>
        Expression GetInstanceExpression(ParameterExpression registrationContext);
    }

    internal abstract class CreatorBase : ICreator
    {
        public virtual Type Type { get; protected set; }
        protected IRegistrationContext parentContext;

        public CreatorBase(IRegistrationContext parentContext)
        {
            this.parentContext = parentContext;
        }

        // Common utility method
        protected Expression CompleteExpressionFromCreator(Expression creator, ParameterExpression registrationContext)
        {
            var instanceVar = Expression.Variable(this.Type, "instance");
            var assignment = Expression.Assign(instanceVar, creator);

            var buildUpExpression = this.parentContext.GetBuilderUpper(this.Type).GetExpression(instanceVar, registrationContext);

            // We always start with:
            // var instance = new Class(.....)
            // instance.Property1 = new ....
            // instance.Property2 = new ....
            var blockItems = new List<Expression>() { assignment, buildUpExpression };
            // If it implements IInjectionAware, follow that up with:
            // instance.ParametersInjected
            if (typeof(IInjectionAware).IsAssignableFrom(this.Type))
                blockItems.Add(Expression.Call(instanceVar, typeof(IInjectionAware).GetMethod("ParametersInjected")));
            // Final appearance of instanceVar, as this sets the return value of the block
            blockItems.Add(instanceVar);
            var completeExpression = Expression.Block(new[] { instanceVar }, blockItems);
            return completeExpression;
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
    }

    // Sealed so Code Analysis doesn't moan about us setting the virtual Type property
    internal sealed class TypeCreator : CreatorBase
    {
        private readonly string _attributeKey;
        public string AttributeKey
        {
            get { return this._attributeKey; }
        }
        private Expression creationExpression;

        public TypeCreator(Type type, IRegistrationContext parentContext) : base(parentContext)
        {
            this.Type = type;

            // Use the key from InjectAttribute (if present), and let someone else override it if they want
            var attribute = (InjectAttribute)type.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
            if (attribute != null)
                this._attributeKey = attribute.Key;
        }

        private string KeyForParameter(ParameterInfo parameter)
        {
            var attribute = parameter.GetCustomAttributes(typeof(InjectAttribute)).FirstOrDefault() as InjectAttribute;
            return attribute == null ? null : attribute.Key;
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.creationExpression != null)
                return this.creationExpression;

            // Find the constructor which has the most parameters which we can fulfill, accepting default values which we can't fulfill
            ConstructorInfo ctor;
            var ctorsWithAttribute = this.Type.GetConstructors().Where(x => x.GetCustomAttributes(typeof(InjectAttribute), false).Any()).ToList();
            if (ctorsWithAttribute.Count > 1)
            {
                throw new StyletIoCFindConstructorException(String.Format("Found more than one constructor with [Inject] on type {0}.", this.Type.Description()));
            }
            else if (ctorsWithAttribute.Count == 1)
            {
                ctor = ctorsWithAttribute[0];
                var key = ((InjectAttribute)ctorsWithAttribute[0].GetCustomAttribute(typeof(InjectAttribute), false)).Key;
                var cantResolve = ctor.GetParameters().Where(p => !this.parentContext.CanResolve(new TypeKey(p.ParameterType, key)) && !p.HasDefaultValue).FirstOrDefault();
                if (cantResolve != null)
                    throw new StyletIoCFindConstructorException(String.Format("Found a constructor with [Inject] on type {0}, but can't resolve parameter '{1}' (of type {2}, and doesn't have a default value).", this.Type.Description(), cantResolve.Name, cantResolve.ParameterType.Description()));
            }
            else
            {
                ctor = this.Type.GetConstructors()
                    .Where(c => c.GetParameters().All(p => this.parentContext.CanResolve(new TypeKey(p.ParameterType, this.KeyForParameter(p))) || p.HasDefaultValue))
                    .OrderByDescending(c => c.GetParameters().Count(p => !p.HasDefaultValue))
                    .FirstOrDefault();

                if (ctor == null)
                {
                    throw new StyletIoCFindConstructorException(String.Format("Unable to find a constructor for type {0} which we can call.", this.Type.Description()));
                }
            }

            // If we get circular dependencies, we'll just blow the stack. They're a pain to resolve.

            // If there parameter's got an InjectAttribute with a key, use that key to resolve
            var ctorParams = ctor.GetParameters().Select(x =>
            {
                var key = this.KeyForParameter(x);
                if (this.parentContext.CanResolve(new TypeKey(x.ParameterType, key)))
                {
                    try
                    {
                        return this.parentContext.GetExpression(new TypeKey(x.ParameterType, key), registrationContext, true);
                    }
                    catch (StyletIoCRegistrationException e)
                    {
                        throw new StyletIoCRegistrationException(String.Format("{0} Required by parameter '{1}' of type {2} (which is a {3}).", e.Message, x.Name, this.Type.Description(), x.ParameterType.Description()), e);
                    }
                }
                // For some reason we need this cast...
                return Expression.Convert(Expression.Constant(x.DefaultValue), x.ParameterType);
            });

            var creator = Expression.New(ctor, ctorParams);

            var completeExpression = this.CompleteExpressionFromCreator(creator, registrationContext);

            Interlocked.CompareExchange(ref this.creationExpression, completeExpression, null);
            return this.creationExpression;
        }
    }

    // Sealed for consistency with TypeCreator
    internal sealed class FactoryCreator<T> : CreatorBase
    {
        private readonly Func<IRegistrationContext, T> factory;

        public override Type Type { get { return typeof(T); } }

        public FactoryCreator(Func<IRegistrationContext, T> factory, IRegistrationContext parentContext)
            : base(parentContext)
        {
            this.factory = factory;
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            // Unfortunately we can't cache the result of this, as it relies on registrationContext
            var expr = (Expression<Func<IRegistrationContext, T>>)(ctx => this.factory(ctx));
            var invoked = Expression.Invoke(expr, registrationContext);

            var completeExpression = this.CompleteExpressionFromCreator(invoked, registrationContext);
            return completeExpression;
        }
    }

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
