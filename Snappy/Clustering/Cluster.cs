using Snappy.ValueObjects;
using System.Collections.Generic;

namespace Snappy.Clustering
{
    public class Cluster<T>
    {
        public Coord Centroid { get; set; }
        public List<T> Values { get; set; }

        public Cluster(Coord centroid, List<T> data)
        {
            Centroid = centroid;
            Values = data;
        }
    }
}