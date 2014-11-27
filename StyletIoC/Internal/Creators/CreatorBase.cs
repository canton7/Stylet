using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Creators
{
    /// <summary>
    /// Base class for all ICreators (which want to use it). Provides convenience
    /// </summary>
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
            // instance.ParametersInjected()
            if (typeof(IInjectionAware).IsAssignableFrom(this.Type))
                blockItems.Add(Expression.Call(instanceVar, typeof(IInjectionAware).GetMethod("ParametersInjected")));
            // Final appearance of instanceVar, as this sets the return value of the block
            blockItems.Add(instanceVar);
            var completeExpression = Expression.Block(new[] { instanceVar }, blockItems);
            return completeExpression;
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
    }
}
