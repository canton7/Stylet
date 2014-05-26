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
        /// <param name="comparer">Comparer, which takes two T instances and returns true if they are equal</param>
        public LambdaComparer(Func<T, T, bool> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            this.comparer = comparer;
        }

        /// <summary>
        /// Determines whether the specified objects are equal
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return this.comparer(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object
        /// </summary>
        /// <param name="obj">The System.Object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
