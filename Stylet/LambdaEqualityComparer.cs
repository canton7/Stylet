using System;
using System.Collections;
using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// IEqualityComparer{T} implementation which uses a Lambda
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        private Func<T, T, bool> equalityComparer;

        /// <summary>
        /// Create a new LambdaEqualityComparer{T}
        /// </summary>
        /// <param name="equalityComparer">Comparer, which takes two T instances and returns true if they are equal</param>
        public LambdaEqualityComparer(Func<T, T, bool> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("comparer");
            this.equalityComparer = equalityComparer;
        }

        /// <summary>
        /// Determines whether the specified objects are equal
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return this.equalityComparer(x, y);
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

        /// <summary>
        /// Determines whether the specified objects are equal
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        bool IEqualityComparer.Equals(object x, object y)
        {
            if (!(x is T) || !(y is T))
                return false;
            return this.equalityComparer((T)x, (T)y);
        }

        /// <summary>
        /// Returns a hash code for the specified object
        /// </summary>
        /// <param name="obj">The System.Object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}
