using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// IEqualityComparer{T} implementation which uses a Lambda
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        Func<T, T, bool> comparer;

        /// <summary>
        /// Create a new LambdaComparer{T}
        /// </summary>
        /// <param name="comparer">Comparer, which takes two {T} instances and returns true if they are equal</param>
        public LambdaComparer(Func<T, T, bool> comparer)
        {
            this.comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return this.comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
