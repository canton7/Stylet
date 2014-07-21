using System;
using System.Collections;
using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// IComparer{T} implementation which uses a lambda
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaComparer<T> : IComparer<T>, IComparer
    {
        private Func<T, T, int> comparer;

        /// <summary>
        /// Create a new LambdaComparer{T} instance
        /// </summary>
        /// <param name="comparer">Comparer to use. Return less than 0 if arg1 is less than arg2, 0 if they're equal, greater than zero otherwise</param>
        public LambdaComparer(Func<T, T, int> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            this.comparer = comparer;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.Value Meaning Less
        /// than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.</returns>
        public int Compare(T x, T y)
        {
            return this.comparer(x, y);
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.Value Meaning Less
        /// than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.</returns>
        int IComparer.Compare(object x, object y)
        {
            if (!(x is T) || !(y is T))
                throw new ArgumentException("Either x or y isn't a T");
            return this.comparer((T)x, (T)y);
        }
    }
}
