using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Clustering
{
    public class FuzzyCMeans<T>
    {
        // number of clusters
        private int _c { get; set; }

        // fuzziness parameter
        private double _m { get; set; }

        private int _maxiters { get; set; }

        public FuzzyCMeans(int c, double m, int maxiters)
        {
            _c = c;
            _m = m;
            _maxiters = maxiters;
        }

        public List<Cluster<T>> Cluster(List<Coord> points, List<T> data)
        {
            int n = points.Count();
            // we need n x c - many random numbers to initialize membership
            Random random = new Random(7);

            double[,] U = new double[n, _c];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < _c; j++)
                {
                    U[i, j] = random.NextDouble();
                }
            }
            List<Coord> centers = new List<Coord>();

            double lowerLat = points.Min(x => x.Latitude);
            double lowerLon = points.Min(x => x.Longitude);

            double upperLat = points.Max(x => x.Latitude);
            double upperLon = points.Max(x => x.Longitude);

            for (int i = 0; i < _c; i++)
            {
                double RandomLat = random.NextDouble() * (upperLat - lowerLat) + lowerLat;
                double RandomLon = random.NextDouble() * (upperLon - lowerLon) + lowerLon;
                Coord randomCenter = new Coord(RandomLat, RandomLon);

                centers.Add(randomCenter);
            }

            for (int p = 0; p < _maxiters; p++)
            {
                //update centers
                for (int i = 0; i < _c; i++)
                {
                    double firstvalue_lat = SumColumnExponentiatedAndMultiplyWithData_Latitude(U, points, i, _m);
                    double firstvalue_lon = SumColumnExponentiatedAndMultiplyWithData_Longitude(U, points, i, _m);
                    double secondvalue = SumColumnExponentiated(U, i, _m); //sum over the n rows of U raised to power m
                    if (secondvalue < 0.00001)
                    {
                        secondvalue = 0.00001;
                    }
                    //putting this in to see what happens
                    double new_lat = firstvalue_lat / secondvalue;
                    double new_lon = firstvalue_lon / secondvalue;
                    if (!Double.IsNaN(new_lat) && !Double.IsNaN(new_lat))
                    {
                        centers[i].Latitude = new_lat;
                        centers[i].Longitude = new_lon;
                    }
                    else
                    {
                        //Console.WriteLine("Paused");
                    }
                }

                //update membership matrix U
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < _c; j++)
                    {
                        //double theijconstantweneed = Math.Pow(DistanceFunctions.HaversineDistance(points[i], centers[j]).DistanceInMeters, 2 / (_m - 1));
                        double theijconstantweneed = Math.Pow(DistanceFunctions.HaversineDistance(points[i], centers[j]).DistanceInMeters, 2 / (_m - 1));
                        //double theijconstantweneed = Math.Pow(DistanceFunctions.LawOfCosinesDistance(points[i], centers[j]).DistanceInMeters, 2 / (_m - 1));
                        //double theijconstantweneed = Math.Pow(DistanceFunctions.ApproximateDistance(points[i], centers[j]).DistanceInMeters, 2 / (_m - 1));
                        double denominator = 0.0;
                        for (int k = 0; k < _c; k++)
                        {
                            //denominator += theijconstantweneed / Math.Pow(DistanceFunctions.HaversineDistance(points[i], centers[k]).DistanceInMeters, 2 / (_m - 1));
                            denominator += theijconstantweneed / Math.Pow(DistanceFunctions.HaversineDistance(points[i], centers[k]).DistanceInMeters, 2 / (_m - 1));
                            //denominator += theijconstantweneed / Math.Pow(DistanceFunctions.LawOfCosinesDistance(points[i], centers[k]).DistanceInMeters, 2 / (_m - 1));
                            //denominator += theijconstantweneed / Math.Pow(DistanceFunctions.ApproximateDistance(points[i], centers[k]).DistanceInMeters, 2 / (_m - 1));
                        }
                        //if (denominator == 0)
                        //{
                        //    denominator = 0.00001;
                        //}
                        U[i, j] = 1 / denominator;
                    }
                }
            }
            var asdsjhf = new List<T>[_c];

            for (int i = 0; i < _c; i++)
            {
                asdsjhf[i] = new List<T>();
            }
            for (int i = 0; i < n; i++)
            {
                int k = 0;
                var not_smallest = U[i, k];
                for (int j = 0; j < _c; j++)
                {
                    if (U[i, j] > not_smallest)
                    {
                        not_smallest = U[i, j];
                        k = j;
                    }
                }
                asdsjhf[k].Add(data[i]);
            }
            var retstuff = new List<Cluster<T>>();
            for (int j = 0; j < _c; j++)
            {
                var gjhh = new Cluster<T>(centers[j], asdsjhf[j]);
                retstuff.Add(gjhh);
            }

            return retstuff;
        }

        public List<Cluster<T>> ComputeV2(List<Coord> points, List<T> data)
        {
            return new List<Cluster<T>>();
        }

        //Helper methods
        //private double SumColumnExponentiated(double[,] arr, int i, double m) => Enumerable.Range(0, arr.GetLength(1)).Aggregate(seed: 0.0, func: (state, j) => state + Math.Pow(arr[i, j], m));
        private double SumColumnExponentiated(double[,] arr, int column, double m)
        {
            double total_sum = 0.0;
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                total_sum += Math.Pow(arr[i, column], m);
            }
            return total_sum;
        }

        //private double SumColumnExponentiatedAndMultiplyWithData_Latitude(double[,] arrU, List<Coord> arrX, int i, double m) => Enumerable.Range(0, arrU.GetLength(1)).Aggregate(seed: 0.0, func: (state, j) => state + arrX[j].Latitude * Math.Pow(arrU[i, j], m));
        private double SumColumnExponentiatedAndMultiplyWithData_Latitude(double[,] arrU, List<Coord> arrX, int column, double m)
        {
            double total_sum = 0.0;
            for (int i = 0; i < arrU.GetLength(0); i++)
            {
                total_sum += Math.Pow(arrU[i, column], m) * arrX[i].Latitude;
            }
            return total_sum;
        }

        //*private double SumColumnExponentiatedAndMultiplyWithData_Longitude(double[,] arrU, List<Coord> arrX, int i, double m)*/ => Enumerable.Range(0, arrU.GetLength(1)).Aggregate(seed: 0.0, func: (state, j) => state + arrX[j].Longitude * Math.Pow(arrU[i, j], m));
        private double SumColumnExponentiatedAndMultiplyWithData_Longitude(double[,] arrU, List<Coord> arrX, int column, double m)
        {
            double total_sum = 0.0;
            for (int i = 0; i < arrU.GetLength(0); i++)
            {
                total_sum += Math.Pow(arrU[i, column], m) * arrX[i].Longitude;
            }
            return total_sum;
        }
    }
}