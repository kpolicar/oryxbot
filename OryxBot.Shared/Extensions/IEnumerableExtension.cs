using System;
using System.Collections.Generic;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Extensions
{
    public static class IEnumerableExtension
    {
        public static ITwoWayEnumerator<T> GetTwoWayEnumerator<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new TwoWayEnumerator<T>(source.GetEnumerator());
        }
    }
}
