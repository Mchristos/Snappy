using Snappy.Config;
using Snappy.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.ValueObjects
{
    public class Coord : IEquatable<Coord>
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        private int _hashCode { get; set; }

        public Coord(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
            _hashCode = GenerateHash();
        }


        private int GenerateHash()
        {
            var latInt = (uint)(Latitude * 1000000);
            var lonInt = (uint)(Longitude * 1000000);
            var resultInt = latInt + checked(lonInt << 16);
            return (int)resultInt;
        }

        public override int GetHashCode()
        {
            return this._hashCode;
        }

        public bool Equals(Coord other)
        {
            // Points are equals if:
            // 1. They hash to the same value.
            // 2. The square distance between them is less than the given constant.
            if (this._hashCode == other.GetHashCode())
            {
                double threshold = Constants.Distance_Threshold_For_Point_Equality_In_Meters;
                return (this.HaversineDistance(other).DistanceInMeters < threshold);
            }
            else
            {
                return false;
            }
        }
    }
}
