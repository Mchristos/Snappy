using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.DataStructures
{
    public class ProbabilityVector<T> : Dictionary<T, double>
    {
        public ProbabilityVector() : base()
        {
        }

        public ProbabilityVector(IEnumerable<T> collection) : base()
        {
            if (collection == null) { throw new NullReferenceException("Collection is null"); }
            int count = collection.Count();
            if (count == 0) { throw new ArgumentException("Collection is empty"); }
            foreach (var item in collection)
            {
                this[item] = 1 / count;
            }
        }

        public ProbabilityVector<T> Normalize()
        {
            double sum = GetSum();
            if (sum > 0)
            {
                var result = new ProbabilityVector<T>();
                foreach (var key in this.Keys)
                {
                    result[key] = this[key] / sum;
                }
                return result;
            }
            else
            {
                return this;
            }
        }

        public double GetSum()
        {
            return this.Values.Sum();
        }

        public T GetMostProbableItem()
        {
            if (this.Count == 0) { throw new Exception("Probability vector is empty"); }
            return this.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        }
    }
}