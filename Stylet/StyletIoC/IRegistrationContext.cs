using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    public interface IRegistrationContext : IContainer
    {
        BuilderUpper GetBuilderUpper(Type type);
        bool CanResolve(TypeKey typeKey);
        Expression GetExpression(TypeKey typeKey, ParameterExpression registrationContext, bool searchGetAllTypes);
        IRegistrationCollection GetRegistrations(TypeKey typeKey, bool searchGetAllTypes);

        event EventHandler Disposing;
    }
}
