using Snappy.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class MathExtensions
    {
        public static double ToRadians(this double degrees)
        {
            return (degrees * Math.PI) / 180;
        }

        public static double ToDegrees(this double radians)
        {
            return (radians * 180) / Math.PI;
        }

        public static double Haversine(double radians)
        {
            return Math.Pow(Math.Sin(radians / 2), 2);
        }

        public static double MetersToDeltaLat(double meters)
        {
            // meters is an arc length. theta = l / r
            return (meters / Constants.Earth_Radius_In_Meters).ToDegrees();
        }

        public static double MetersToDeltaLng(double meters, double refLatInDegrees)
        {
            var refLatInRadians = refLatInDegrees.ToRadians();
            var radius = Constants.Earth_Radius_In_Meters * Math.Cos(refLatInRadians);
            return (meters / radius).ToDegrees();
        }


    }
}