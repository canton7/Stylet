using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    internal interface ICreator
    {
        Type Type { get; }
        Expression GetInstanceExpression();
    }

    internal abstract class CreatorBase : ICreator
    {
        public virtual Type Type { get; protected set; }
        protected StyletIoCContainer container;
        public abstract Expression GetInstanceExpression();

        public CreatorBase(StyletIoCContainer container)
        {
            this.container = container;
        }

        // Common utility method
        protected Expression CompleteExpressionFromCreator(Expression creator)
        {
            var instanceVar = Expression.Variable(this.Type, "instance");
            var assignment = Expression.Assign(instanceVar, creator);

            var buildUpExpression = this.container.GetBuilderUpper(this.Type).GetExpression(instanceVar);

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
    }

    // Sealed so Code Analysis doesn't moan about us setting the virtual Type property
    internal sealed class TypeCreator : CreatorBase
    {
        public string AttributeKey { get; private set; }
        private Expression creationExpression;

        public TypeCreator(Type type, StyletIoCContainer container) : base(container)
        {
            this.Type = type;

            // Use the key from InjectAttribute (if present), and let someone else override it if they want
            var attribute = (InjectAttribute)type.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
            if (attribute != null)
                this.AttributeKey = attribute.Key;
        }

        private string KeyForParameter(ParameterInfo parameter)
        {
            var attributes = parameter.GetCustomAttributes(typeof(InjectAttribute));
            if (attributes == null)
                return null;
            var attribute = (InjectAttribute)attributes.FirstOrDefault();
            return attribute == null ? null : attribute.Key;
        }

        public override Expression GetInstanceExpression()
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
                var cantResolve = ctor.GetParameters().Where(p => !this.container.CanResolve(new TypeKey(p.ParameterType, key)) && !p.HasDefaultValue).FirstOrDefault();
                if (cantResolve != null)
                    throw new StyletIoCFindConstructorException(String.Format("Found a constructor with [Inject] on type {0}, but can't resolve parameter '{1}' (which doesn't have a default value).", this.Type.Description(), cantResolve.Name));
            }
            else
            {
                ctor = this.Type.GetConstructors()
                    .Where(c => c.GetParameters().All(p => this.container.CanResolve(new TypeKey(p.ParameterType, this.KeyForParameter(p))) || p.HasDefaultValue))
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
                if (this.container.CanResolve(new TypeKey(x.ParameterType, key)))
                {
                    try
                    {
                        return this.container.GetExpression(new TypeKey(x.ParameterType, key), true);
                    }
                    catch (StyletIoCRegistrationException e)
                    {
                        throw new StyletIoCRegistrationException(String.Format("{0} Required by parameter '{1}' of type {2}.", e.Message, x.Name, this.Type.Description()), e);
                    }
                }
                // For some reason we need this cast...
                return Expression.Convert(Expression.Constant(x.DefaultValue), x.ParameterType);
            });

            var creator = Expression.New(ctor, ctorParams);

            var completeExpression = this.CompleteExpressionFromCreator(creator);

            this.creationExpression = completeExpression;
            return completeExpression;
        }
    }

    // Sealed for consistency with TypeCreator
    internal sealed class FactoryCreator<T> : CreatorBase
    {
        private Func<StyletIoCContainer, T> factory;
        private Expression instanceExpression;

        public override Type Type { get { return typeof(T); } }

        public FactoryCreator(Func<StyletIoCContainer, T> factory, StyletIoCContainer container) : base(container)
        {
            this.factory = factory;
        }

        public override Expression GetInstanceExpression()
        {
            if (this.instanceExpression != null)
                return this.instanceExpression;

            var expr = (Expression<Func<T>>)(() => this.factory(this.container));
            var invoked = Expression.Invoke(expr, null);

            var completeExpression = this.CompleteExpressionFromCreator(invoked);

            this.instanceExpression = completeExpression;
            return completeExpression;
        }
    }
}
