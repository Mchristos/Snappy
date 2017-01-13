using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Clustering
{
    public class NaiveDBSCAN<T>
    {
        private double _radius { get; set; }

        public NaiveDBSCAN(double radiusInMeters)
        {
            _radius = radiusInMeters;
        }

        public List<Cluster<T>> Cluster(List<T> data, Func<T, Coord> getCoordinate)
        {
            var result = new List<Cluster<T>>();

            HashSet<T> stopSet = new HashSet<T>(data);
            while (stopSet.Count != 0)
            {
                // Take the first item in the set and make a singleton set
                var singletonSet = new HashSet<T>() { stopSet.First() };
                // Take the closure, i.e. recursively grow the set to get the full cluster
                var clusterSet = singletonSet.Closure(stopSet, (x, y) => getCoordinate(x).HaversineDistance(getCoordinate(y)).DistanceInMeters < _radius);

                // Find the centroid of the cluster and add the cluster to the result.
                var centroid = clusterSet.Select(x => getCoordinate(x)).ComputeCenter();
                var cluster = new Cluster<T>(centroid, clusterSet.ToList());
                result.Add(cluster);

                // Remove the clustered items and continue the algorithm on the rest of the unclustered points
                foreach (var item in clusterSet)
                {
                    stopSet.Remove(item);
                }
            }

            return result;
        }
    }
}