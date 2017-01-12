using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.ValueObjects;

namespace Snappy.Clustering
{
    class EstimatedCMeans<T>
    {
        //private List<T> _data { get; set; }
        public int RegionRadius { get; set; }
        public int ClusterRaduis { get; set; }
        public int DBSCANMinPOints { get; set; }
        //Func<T, Coord> getCoord { get; set; }
        public double Fuzzyness { get; set; } //Default to 1.85
        public int MaxItters { get; set; } //Default to 30

        public EstimatedCMeans(int RegionRadius, int ClusterRaduis, double Fuzzyness, int MaxItters, int DBSCANminPOints)
        {
            this.RegionRadius = RegionRadius;
            this.ClusterRaduis = ClusterRaduis;
            this.Fuzzyness = Fuzzyness;
            this.MaxItters = MaxItters;
            this.DBSCANMinPOints = DBSCANMinPOints;
        }            

        public List<Cluster<T>> Cluster(List<T> data, Func<T, Coord> getCoord) 
        {
            var DensityClustering = new DBSCAN<T>(data, DBSCANMinPOints, RegionRadius);
            List<Cluster<T>> LargeRegions = DensityClustering.Cluster(getCoord);
            List<Cluster<T>> ListofClusters = new List<Cluster<T>>();

           // Fuzzy Cmeans on Region

            Parallel.ForEach(LargeRegions, smallerRegion =>
            {
                var smallerCluster = smallerRegion.Values;
                var DensityClusterinngP2 = new DBSCAN<T>(smallerCluster, DBSCANMinPOints, ClusterRaduis);
                var EstimatedC = DensityClusterinngP2.Cluster(getCoord);
                int expectedC = EstimatedC.Count();

                var fuzzyC = new FuzzyCMeans<T>(expectedC, Fuzzyness, MaxItters);
                List<Cluster<T>> clusters = fuzzyC.Cluster(smallerCluster.Select(x => getCoord(x)).ToList(), smallerCluster);
                ListofClusters.AddRange(clusters);
            });

            return ListofClusters;
        }

    }
}
