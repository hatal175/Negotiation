using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public static class IEnumerableUtils
    {
        private static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            return source.Except(item.SingleItemAsEnumerable());
        }

        public static TSource ArgMax<TSource, TArgument>(this IEnumerable<TSource> source, Func<TSource, TArgument> converter)
            where TArgument : IComparable<TArgument>
        {
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence has no elements.");

            TSource maxItem = enumerator.Current;
            if (!enumerator.MoveNext())
                return maxItem;

            TArgument maxValue = converter(maxItem);
            do
            {
                TSource currentItem;
                TArgument currentMaxValue;

                if ((currentMaxValue = converter(currentItem = enumerator.Current)).CompareTo(maxValue) > 0)
                {
                    maxItem = currentItem;
                    maxValue = currentMaxValue;
                }
            }
            while (enumerator.MoveNext());
            return maxItem;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }
    }
}