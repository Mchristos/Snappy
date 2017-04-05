using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Partitions a list into specified number of partitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="partitions"></param>
        /// <param name="exclusive"> Specifies whether partitioning is exclusive or inclusive </param>
        /// <returns></returns>
        public static List<List<T>> ExclusivePartition<T>(this IEnumerable<T> source, int partitions)
        {
            int count = source.Count();
            var size = count / partitions;
            if (size < 1) return new List<List<T>>() { source.ToList() };            
            if(count % size != 0)
            {
                size += 1;
            }
            var result = new List<List<T>>(); 
            while (source.Count() > 1)
            {
                var part = source.Take(size);
                result.Add(part.ToList());
                source = source.Skip(size);
            }
            return result;
        }



        /// <summary>
        /// Breaks up a list at the values where the predicate evaluates to true. the breaking point is included
        /// in "both sides" of the partitioning
        /// e.g. [0,0,1,0,0,1,0,0] with predicate "equals 1" ==> [ [0,0,1],[1,0,0,1],[1,0,0] ]
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<List<T>> InclusivePartitioning<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source.Count() < 3) { return new List<List<T>>() { source.ToList() }; };
            var result = new List<List<T>>();
            int i = 1;
            var list = source.ToList();           
            while (i < list.Count)
            {
                if (i == list.Count - 1)
                {
                    result.Add(list);
                    break;
                }
                if (predicate(list[i]))
                {
                    result.Add(list.Take(i + 1).ToList());
                    result.AddRange(InclusivePartitioning(source.Skip(i), predicate));
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

        /// <summary>
        /// Computes the closure of a set in a superset with a given equivalence relation. That is, it computes an "expanded" version of the input set by recursively adding any points in the superset that are equivalent to something in the given set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="superset"></param>
        /// <param name="equivalenceRelation"> </param>
        /// <returns></returns>
        public static HashSet<T> Closure<T>(this HashSet<T> input, HashSet<T> superset, Func<T, T, bool> equivalenceRelation)
        {
            // Initialize result 
            HashSet<T> result = new HashSet<T>();

            // Add any items in the superset that are equivalent to something in the input 
            foreach (var item1 in input)
            {
                foreach (var item2 in superset)
                {
                    if (equivalenceRelation(item1, item2))
                    {
                        result.Add(item2);
                    }
                }
            }

            // Recursively take the closure of the result if the result was bigger than the input. If it did not "grow", and the recursion.
            bool expanded = (result.Count > input.Count);
            if (expanded)
            {
                return result.Closure(superset, equivalenceRelation);
            }
            else
            {
                return result;
            }
        }



    }
}