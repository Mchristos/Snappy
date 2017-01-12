using Snappy.Config;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;

namespace Snappy.Functions
{
    public static class DistanceFunctions
    {
        public static Distance HaversineDistance(this Coord coord1, Coord coord2)
        {
            return HaversineDistance(coord1.Latitude, coord1.Longitude, coord2.Latitude, coord2.Longitude);
        }

        public static Distance HaversineDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            double delta_lat = (latitude2 - latitude1).ToRadians();
            double delta_lon = (longitude2 - longitude1).ToRadians();
            double lat_1 = latitude1.ToRadians();
            double lat_2 = latitude2.ToRadians();
            double central_angle = MathExtensions.Haversine(delta_lat) + MathExtensions.Haversine(delta_lon) * Math.Cos(lat_1) * Math.Cos(lat_2);
            return Distance.FromMeters(2 * Constants.Earth_Radius_In_Meters * Math.Asin(Math.Sqrt(central_angle)));
        }

        public static Distance LawOfCosinesDistance(this Coord coord1, Coord coord2)
        {
            return LawOfCosinesDistance(coord1.Latitude, coord1.Longitude, coord2.Latitude, coord2.Longitude);
        }
        public static Distance LawOfCosinesDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            return ValueObjects.Distance.FromMeters(Math.Acos(Math.Sin(latitude1) * Math.Sin(latitude2) + Math.Cos(latitude1) * Math.Cos(latitude2) * Math.Cos(longitude2 - longitude1)) * Constants.Earth_Radius_In_Meters);
        }
        public static double FasterHaversineDistance(Coord pointA, Coord pointB)
        {
            var g1 = new CartesianPoint3D(pointA.Latitude, pointA.Longitude);
            var g2 = new CartesianPoint3D(pointB.Latitude, pointB.Longitude);
            return FasterHaversineDistance(g1, g2);
        }

        private static double FasterHaversineDistance(CartesianPoint3D g1, CartesianPoint3D g2)
        {
            double dX = g1.X - g2.X;
            double dY = g1.Y - g2.Y;
            double dZ = g1.Z - g2.Z;

            double r = Math.Sqrt(dX * dX + dY * dY + dZ * dZ);

            return Constants.Earth_Diameter_In_Meters* Math.Asin(r);
        }



        public static Distance ComputeLength(this List<Coord> polyline)
        {
            Distance result = Distance.Zero;
            for (int i = 1; i < polyline.Count; i++)
            {
                result += polyline[i].HaversineDistance(polyline[i - 1]);
            }
            return result;
        }

        public static Distance ComputeCumulativeDistanceFromStart(List<Coord> geometry, int position, Coord projection)
        {
            if (position > geometry.Count - 2 || position < 0) { throw new ArgumentException("Invalid position in geometry"); }

            Distance result = Distance.Zero;
            for (int i = 1; i <= position; i++)
            {
                result += geometry[i].HaversineDistance(geometry[i - 1]);
            }
            result += geometry[position].HaversineDistance(projection);

            return result;
        }

        public static Distance ComputeDistanceToEnd(List<Coord> geometry, int position, Coord projection)
        {
            if (position > geometry.Count - 2 || position < 0) { throw new ArgumentException("Invalid position in geometry"); }

            Distance result = Distance.Zero;
            result += projection.HaversineDistance(geometry[position + 1]);
            for (int i = position + 2 ; i < geometry.Count; i++)
            {
                result += geometry[i].HaversineDistance(geometry[i - 1]);
            }

            return result;
        }
    }
}