using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Clustering
{
    public class DBSCAN<T>
    {
        private List<T> _data { get; set; }        
        private int _MinPts { get; set; }
        private double _radius { get; set; }
        private Func<Coord, Coord, double> GetDistance { get; set; }
        private Func<Coord, Coord, int, bool> IsCloseEnough { get; set; }

        public DBSCAN(int minPoints, double radius, Func<Coord, Coord, double> distanceFunction, Func<Coord, Coord, int, bool> isCloseEnough)
        {
            _MinPts = minPoints;
            _radius = radius;
            GetDistance = distanceFunction;
            IsCloseEnough = isCloseEnough;
        }

        public DBSCAN(int MinPts, double radius)
        {
            _MinPts = MinPts;
            _radius = radius;
            //GetDistance = (a, b) => DistanceFunctions.HaversineDistance(a, b).DistanceInMeters;
            GetDistance = (a, b) => DistanceFunctions.FasterHaversineDistance(a, b);            
            IsCloseEnough = (a, b, c) => (GetDistance(a, b) < (Math.Exp(-Math.Pow(c,0.6)/40) * _radius));
        }

        public List<Cluster<T>> Cluster(List<T> data, Func<T, Coord> getCoordinate)
        {
            _data = data;
            List<Cluster<T>> returnList = new List<Cluster<T>>();
            Dictionary<Coord, bool> visitedDictionary = new Dictionary<Coord, bool>();
            foreach(T d in _data)
            {
                visitedDictionary[getCoordinate(d)] = false;
            }
            Dictionary<Coord, bool> clusteredDictionary = new Dictionary<Coord, bool>();
            foreach (T d in _data)
            {
                clusteredDictionary[getCoordinate(d)] = false;
            }
            Dictionary<Coord, bool> noiseDictionary = new Dictionary<Coord, bool>(); //should also construct clusters for noise points
            foreach (T d in _data)
            {
                noiseDictionary[getCoordinate(d)] = false;
            }

            Cluster<T> cluster;// = new Cluster<T>(getCoordinate(_data.FirstOrDefault()), new List<T>());
            foreach(T p in _data)
            {
                if(visitedDictionary[getCoordinate(p)])
                {
                    continue;
                }
                visitedDictionary[getCoordinate(p)] = true;
                List <T> neighbourPoints = regionQuery(p, getCoordinate);
                if(neighbourPoints.Count() < _MinPts)
                {
                    noiseDictionary[getCoordinate(p)] = true;
                }
                else
                {
                    cluster = new Cluster<T>(getCoordinate(_data.FirstOrDefault()), new List<T>());
                    cluster = expandCluster(p, neighbourPoints, cluster, getCoordinate, ref clusteredDictionary, ref visitedDictionary);
                    returnList.Add(cluster);
                }
            }
            return returnList;
        }

        private Cluster<T> expandCluster(T p, List<T> neighbourPoints, Cluster<T> cluster, Func<T, Coord> getCoordinate, ref Dictionary<Coord, bool> clusteredDictionary, ref Dictionary<Coord, bool> visitedDictionary)
        {
            //throw new NotImplementedException();
            cluster.Values.Add(p);
            for(int i = 0; i < neighbourPoints.Count(); i++)
            {
                T p2 = neighbourPoints[i];
                if (!visitedDictionary[getCoordinate(p2)])
                {
                    visitedDictionary[getCoordinate(p2)] = true;
                    int extraCount = 0;
                    List<T> neighbourPoints2 = regionQuery(p2, getCoordinate, neighbourPoints, ref extraCount);
                    if(neighbourPoints2.Count() + extraCount >= _MinPts)
                    {
                        foreach(T x in neighbourPoints2)
                        {
                            if (neighbourPoints.Contains(x))
                            {
                                Console.WriteLine(x.ToString());
                            }
                            else
                            {
                                neighbourPoints.Add(x);
                            }
                        }
                    }
                }
                if (!clusteredDictionary[getCoordinate(p2)])
                {
                    clusteredDictionary[getCoordinate(p2)] = true;
                    cluster.Values.Add(p2);
                }
            }


            #region superfluous stuff
            double avg_lat = 0.0;
            double avg_lon = 0.0;
            foreach(T x in cluster.Values)
            {
                Coord point = getCoordinate(x);
                avg_lat += point.Latitude;
                avg_lon += point.Longitude;
            }
            avg_lat = avg_lat / cluster.Values.Count();
            avg_lon = avg_lon / cluster.Values.Count();
            cluster.Centroid.Latitude = avg_lat;
            cluster.Centroid.Longitude = avg_lon;
            #endregion
            return cluster;

        }

        private List<T> regionQuery(T p, Func<T, Coord> getCoordinate)
        {
            //throw new NotImplementedException();
            List<T> neighbourPoints = new List<T>();
            neighbourPoints.Add(p);
            foreach (T p2 in _data) //this can be optimized
            {
                if (IsCloseEnough(getCoordinate(p), getCoordinate(p2), neighbourPoints.Count()))
                {
                    if (!p2.Equals(p))
                    {
                        neighbourPoints.Add(p2);
                    }
                }
            }
            return neighbourPoints;
        }
        private List<T> regionQuery(T p, Func<T, Coord> getCoordinate, List<T> previousNeighbourPoints, ref int extraCount)
        {
            //throw new NotImplementedException();
            int previousCount = previousNeighbourPoints.Count();
            List<T> neighbourPoints = new List<T>();
            neighbourPoints.Add(p);
            foreach (T p2 in _data) //this can be optimized
            {
                if (IsCloseEnough(getCoordinate(p), getCoordinate(p2), previousCount + neighbourPoints.Count()))
                {
                    if (!p2.Equals(p))
                    {
                        if (!previousNeighbourPoints.Contains(p2)) //make sure Ts can be compared!!
                        {
                            neighbourPoints.Add(p2);
                        }
                        else
                        {
                            extraCount++;
                        }
                    }
                }
            }
            return neighbourPoints;
        }

        private static bool ListContainsPoint(List<T> listOfT, T p, Func<T, Coord> getCoordinate)
        {
            Coord a = getCoordinate(p);
            foreach(T q in listOfT)
            {
                Coord b = getCoordinate(q);
                if (b.Equals(a))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
