using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StyletIoC
{
    internal class BuilderUpper
    {
        private readonly Type type;
        private readonly StyletIoCContainer container;
        private readonly object implementorLock = new object();
        private Action<object> implementor;

        public BuilderUpper(Type type, StyletIoCContainer container)
        {
            this.type = type;
            this.container = container;
        }

        public Expression GetExpression(Expression inputParameterExpression)
        {
            var expressions = this.type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(x => this.ExpressionForMember(inputParameterExpression, x, x.FieldType))
                .Concat(this.type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(x => this.ExpressionForMember(inputParameterExpression, x, x.PropertyType)))
                .Where(x => x != null);

            // Sadly, we can't cache this expression (I think), as it relies on the inputParameterExpression
            // which is likely to change between calls
            // This isn't so bad, so we'll (probably) only need to call this at most twice - once for building up the type on creation,
            // and once for creating the implemtor (which is used in BuildUp())
            if (!expressions.Any())
                return Expression.Empty();
            return Expression.Block(expressions);
        }

        private Expression ExpressionForMember(Expression objExpression, MemberInfo member, Type memberType)
        {
            var attribute = member.GetCustomAttribute<InjectAttribute>(true);
            if (attribute == null)
                return null;

            var memberAccess = Expression.MakeMemberAccess(objExpression, member);
            var memberValue = this.container.GetExpression(new TypeKey(memberType, attribute.Key), true);
            var assign = Expression.Assign(memberAccess, memberValue);
            // Only actually do the assignment if the field/property is currently null
            return Expression.IfThen(Expression.Equal(memberAccess, Expression.Constant(null, memberType)), assign);
        }

        public Action<object> GetImplementor()
        {
            lock (this.implementorLock)
            {
                if (this.implementor != null)
                    return this.implementor;

                var parameterExpression = Expression.Parameter(typeof(object), "inputParameter");
                var typedParameterExpression = Expression.Convert(parameterExpression, this.type);
                var expression = this.GetExpression(typedParameterExpression);
                this.implementor = Expression.Lambda<Action<object>>(this.GetExpression(typedParameterExpression), parameterExpression).Compile();

                return this.implementor;
            }
        }
    }
}
