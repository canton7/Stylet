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
        public virtual RuntimeTypeHandle TypeHandle { get; protected set; }
        protected IRegistrationContext ParentContext { get; set; }

        protected CreatorBase(IRegistrationContext parentContext)
        {
            this.ParentContext = parentContext;
        }

        // Common utility method
        protected Expression CompleteExpressionFromCreator(Expression creator, ParameterExpression registrationContext)
        {
            var type = Type.GetTypeFromHandle(this.TypeHandle);

            var instanceVar = Expression.Variable(type, "instance");
            var assignment = Expression.Assign(instanceVar, creator);

            var buildUpExpression = this.ParentContext.GetBuilderUpper(type).GetExpression(instanceVar, registrationContext);

            // We always start with:
            // var instance = new Class(.....)
            // instance.Property1 = new ....
            // instance.Property2 = new ....
            var blockItems = new List<Expression>() { assignment, buildUpExpression };
            // If it implements IInjectionAware, follow that up with:
            // instance.ParametersInjected()
            if (typeof(IInjectionAware).IsAssignableFrom(type))
                blockItems.Add(Expression.Call(instanceVar, typeof(IInjectionAware).GetMethod("ParametersInjected")));
            // Final appearance of instanceVar, as this sets the return value of the block
            blockItems.Add(instanceVar);
            var completeExpression = Expression.Block(new[] { instanceVar }, blockItems);
            return completeExpression;
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
    }
}
