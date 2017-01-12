using System;

namespace Snappy.ValueObjects
{
    public class CartesianPoint3D
    {
        public double LatitudeInRads { get; set; }
        public double LongitudeInRads { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public CartesianPoint3D(double latitude, double longitude)
        {
            LatitudeInRads = (Math.PI / 180) * latitude;
            LongitudeInRads = (Math.PI / 180) * longitude;
            // Cartesian coordinates, normalized for a sphere of diameter 1.0
            X = 0.5 * Math.Cos(LatitudeInRads) * Math.Sin(LongitudeInRads);
            Y = 0.5 * Math.Cos(LatitudeInRads) * Math.Cos(LongitudeInRads);
            Z = 0.5 * Math.Sin(LatitudeInRads);
        }
    }
}