using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Builder
{
    public interface IRegistrationContext : IContainer
    {
        BuilderUpper GetBuilderUpper(Type type);
        bool CanResolve(Type type, string key);
        Expression GetExpression(Type type, string key, ParameterExpression registrationContext, bool searchGetAllTypes);
        IRegistrationCollection GetRegistrations(Type type, string key, bool searchGetAllTypes);

        event EventHandler Disposing;
    }
}
