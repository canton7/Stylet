using System;
using System.Linq.Expressions;

namespace Stylet
{
    /// <summary>
    /// Useful extension methods on Expressions
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Given a MemberExpression (or MemberExpression wrapped in a UnaryExpression), get the name of the property
        /// </summary>
        /// <typeparam name="TDelegate">Type of the delegate</typeparam>
        /// <param name="propertyExpression">Expression describe the property whose name we want to extract</param>
        /// <returns>Name of the property referenced by the expression</returns>
        public static string NameForProperty<TDelegate>(this Expression<TDelegate> propertyExpression)
        {
            Expression body;
            var expression = propertyExpression.Body as UnaryExpression;
            if (expression != null)
                body = expression.Operand;
            else
                body = propertyExpression.Body;

            var member = body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Property must be a MemberExpression");

            return member.Member.Name;
        }
    }
}
