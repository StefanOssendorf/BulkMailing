using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Extension class for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerableTExtensions {
        /// <summary>
        /// Converts the <paramref name="source"/> to a <see cref="BlockingCollection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Source to convert.</param>
        /// <returns>A new <see cref="BlockingCollection{T}"/>.</returns>
        public static BlockingCollection<T> ToBlockingCollection<T>(this IEnumerable<T> source) {
            Contract.Requires<ArgumentNullException>(source.IsNotNull());
            var col = new BlockingCollection<T>();
            col.AddFromEnumerable(source, true);
            return col;
        }
    }
}