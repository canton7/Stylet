using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public static class ExpressionExtensions
    {
        public static string NameForProperty<TDelegate>(this Expression<TDelegate> propertyExpression)
        {
            Expression body;
            if (propertyExpression.Body is UnaryExpression)
                body = ((UnaryExpression)propertyExpression.Body).Operand;
            else
                body = propertyExpression.Body;

            var member = body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Property must be a MemberExpression");

            return member.Member.Name;
        }
    }
}
