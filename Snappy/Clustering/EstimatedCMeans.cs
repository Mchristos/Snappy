using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.ValueObjects;

namespace Snappy.Clustering
{
    public class EstimatedCMeans<T>
    {
        //private List<T> _data { get; set; }
        public double RegionRadius { get; set; }
        public double ClusterRaduis { get; set; }
        public int DBSCANMinPOints { get; set; }
        //Func<T, Coord> getCoord { get; set; }
        public double Fuzzyness { get; set; } //Default to 1.85
        public int MaxItters { get; set; } //Default to 30

        public EstimatedCMeans(double regionRadius, double clusterRaduis, double fuzzyness, int maxIters, int DBSCANminPoints)
        {
            this.RegionRadius = regionRadius;
            this.ClusterRaduis = clusterRaduis;
            this.Fuzzyness = fuzzyness;
            this.MaxItters = maxIters;
            this.DBSCANMinPOints = DBSCANminPoints;
        }            

        public List<Cluster<T>> Cluster(List<T> data, Func<T, Coord> getCoord) 
        {
            var DensityClustering = new DBSCAN<T>(DBSCANMinPOints, RegionRadius);
            List<Cluster<T>> LargeRegions = DensityClustering.Cluster(data, getCoord);
            List<Cluster<T>> ListofClusters = new List<Cluster<T>>();

           // Fuzzy Cmeans on Region

            Parallel.ForEach(LargeRegions, smallerRegion =>
            {
                var smallerCluster = smallerRegion.Values;
                var DensityClusterinngP2 = new DBSCAN<T>(DBSCANMinPOints, ClusterRaduis);
                var EstimatedC = DensityClusterinngP2.Cluster(smallerCluster, getCoord);
                int expectedC = EstimatedC.Count();

                var fuzzyC = new FuzzyCMeans<T>(expectedC, Fuzzyness, MaxItters);
                List<Cluster<T>> clusters = fuzzyC.Cluster(smallerCluster.Select(x => getCoord(x)).ToList(), smallerCluster);
                ListofClusters.AddRange(clusters);
            });

            return ListofClusters;
        }

    }
}
