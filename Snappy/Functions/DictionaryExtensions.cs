using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Functions
{
    public static class DictionaryExtensions
    {

        public static Dictionary<long, int> CountRepeatedIds(this IEnumerable<long[]> ways)
        {
            var result = new Dictionary<long, int>();
            foreach (var array in ways)
            {
                foreach (var id in array)
                {
                    if (result.ContainsKey(id))
                    {
                        result[id] += 1;
                    }
                    else
                    {
                        result[id] = 1;
                    }
                }
            }
            return result;
        }

        public static List<T> TraceBackSequence<T>(this List<Dictionary<T,T>> prevMemory, T seedValue)
        {
            List<T> backwardsResult = new List<T>() { seedValue };

            T currentValue = seedValue;
            for (int i = prevMemory.Count -1; i >= 0; i--)
            {
                Dictionary<T, T> memory = prevMemory[i];
                if (memory.ContainsKey(currentValue))
                {
                    T prevValue = memory[currentValue];
                    if (prevValue == null) break;
                    backwardsResult.Add(prevValue);
                    currentValue = prevValue;
                }
                else
                {
                    throw new ArgumentException("Error in the input 'prevMemory' ");
                }
            }
            return Enumerable.Reverse(backwardsResult).ToList();
        }

    }
}
