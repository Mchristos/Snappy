using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Breaks up a list at the values where the predicate evaluates to true. the breaking point is included
        /// in "both sides" of the partitioning
        /// e.g. [0,0,1,0,0,1,0,0] with predicate "equals 1" ==> [ [0,0,1],[1,0,0,1],[1,0,0] ]
        /// </summary>
        /// <param name="array"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T[]> InclusivePartitioning<T>(this T[] array, Func<T, bool> predicate)
        {
            if (array.Length < 3) { return new List<T[]>() { array }; };
            var result = new List<T[]>();
            int i = 1;
            while (i < array.Length)
            {
                if (i == array.Length - 1)
                {
                    result.Add(array);
                    break;
                }
                if (predicate(array[i]))
                {
                    result.Add(array.Take(i + 1).ToArray());
                    result.AddRange(InclusivePartitioning(array.Skip(i).ToArray(), predicate));
                    break;
                }
                i += 1;
            }
            return result;
        }

        /// <summary>
        /// Eradicates duplicated neighboring values
        /// e.g. [0,1,2,2,2,3] --> [0,1,2,3]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<T> RemoveDuplicateNeighbors<T>(this List<T> input) where T : IEquatable<T>
        {
            if (input.Count < 2) return input;
            T currentValue = input.First();
            var result = new List<T>() { currentValue };
            for (int i = 1; i < input.Count; i++)
            {
                var value = input[i];
                if (!value.Equals(currentValue))
                {
                    result.Add(value);
                    currentValue = value;
                }
            }
            return result;
        }
    }
}