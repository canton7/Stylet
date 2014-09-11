using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Builder
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
}
