using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    public static class Extensions
    {

        public static DirectedRoad Reverse(this DirectedRoad road)
        {
            return new DirectedRoad(road.End, road.Start, Enumerable.Reverse(road.Geometry).ToList(), road.Name);
        }

        public static string GetNewSquid()
        {
            return Convert
                .ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
        }
        public static bool IsValidLatInDegrees(double input)
        {
            return (-90 <= input) && (input <= 90);
        }
        public static bool IsValidLngInDegrees(double input)
        {
            return (-180 <= input) && (input <= 180);
        }

    }
}
