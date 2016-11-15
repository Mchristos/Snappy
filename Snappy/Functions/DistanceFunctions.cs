using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using Snappy.Config;

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
            if( position > geometry.Count - 1 || position < 0) { throw new ArgumentException("Invalid position in geometry"); }    

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
            if (position > geometry.Count - 1 || position < 0) { throw new ArgumentException("Invalid position in geometry"); }

            Distance result = Distance.Zero;
            for (int i = 1; i <= position; i++)
            {
                result += geometry[i].HaversineDistance(geometry[i - 1]);
            }
            result += geometry[position].HaversineDistance(projection);

            return result;
        }
    }
}