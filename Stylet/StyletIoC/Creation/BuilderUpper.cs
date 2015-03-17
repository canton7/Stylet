using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StyletIoC.Creation
{
    /// <summary>
    /// A BuilderUpper knows how to build up an object - that is, populate all parameters decorated with [Inject]
    /// </summary>
    public class BuilderUpper
    {
        private readonly RuntimeTypeHandle typeHandle;
        private readonly IRegistrationContext parentContext;
        private readonly object implementorLock = new object();
        private Action<IRegistrationContext, object> implementor;

        /// <summary>
        /// Initialises a new instance of the <see cref="BuilderUpper"/> class
        /// </summary>
        /// <param name="typeHandle">Type of object that the BuilderUpper will work on</param>
        /// <param name="parentContext">IRegistrationContext on which this BuilderUpper is registered</param>
        public BuilderUpper(RuntimeTypeHandle typeHandle, IRegistrationContext parentContext)
        {
            this.typeHandle = typeHandle;
            this.parentContext = parentContext;
        }

        /// <summary>
        /// Get an expression which takes an object represented by inputParameterExpression, and returns an expression which builds it up
        /// </summary>
        /// <param name="inputParameterExpression">Expression representing the object instance to build up</param>
        /// <param name="registrationContext">Context which calls this method</param>
        /// <returns>Expression which will build up inputParameterExpression</returns>
        public Expression GetExpression(Expression inputParameterExpression, ParameterExpression registrationContext)
        {
            return this.GetExpression(inputParameterExpression, registrationContext, Type.GetTypeFromHandle(this.typeHandle));
        }

        private Expression GetExpression(Expression inputParameterExpression, ParameterExpression registrationContext, Type type)
        {
            var expressions = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(x => this.ExpressionForMember(inputParameterExpression, x, x.FieldType, registrationContext))
                .Concat(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(x => this.ExpressionForMember(inputParameterExpression, x, x.PropertyType, registrationContext)))
                .Where(x => x != null)
                .ToList();

            // Sadly, we can't cache this expression (I think), as it relies on the inputParameterExpression
            // which is likely to change between calls
            // This isn't so bad, so we'll (probably) only need to call this at most twice - once for building up the type on creation,
            // and once for creating the implemtor (which is used in BuildUp())
            if (expressions.Count == 0)
                return Expression.Empty();
            return Expression.Block(expressions);
        }

        private Expression ExpressionForMember(Expression objExpression, MemberInfo member, Type memberType, ParameterExpression registrationContext)
        {
            var attribute = member.GetCustomAttribute<InjectAttribute>(true);
            if (attribute == null)
                return null;

            var memberAccess = Expression.MakeMemberAccess(objExpression, member);
            var memberValue = this.parentContext.GetSingleRegistration(memberType, attribute.Key, true).GetInstanceExpression(registrationContext);
            var assign = Expression.Assign(memberAccess, memberValue);
            // Only actually do the assignment if the field/property is currently null
            return Expression.IfThen(Expression.Equal(memberAccess, Expression.Constant(null, memberType)), assign);
        }

        /// <summary>
        /// Get a delegate which, given an object, will build it up
        /// </summary>
        /// <returns>Delegate which, when given an object, will build it up</returns>
        public Action<IRegistrationContext, object> GetImplementor()
        {
            lock (this.implementorLock)
            {
                if (this.implementor != null)
                    return this.implementor;

                var type = Type.GetTypeFromHandle(this.typeHandle);

                var parameterExpression = Expression.Parameter(typeof(object), "inputParameter");
                var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
                var typedParameterExpression = Expression.Convert(parameterExpression, type);
                this.implementor = Expression.Lambda<Action<IRegistrationContext, object>>(this.GetExpression(typedParameterExpression, registrationContext, type), registrationContext, parameterExpression).Compile();

                return this.implementor;
            }
        }
    }
}
