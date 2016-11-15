using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.ValueObjects
{
    public class Distance : IComparable<Distance>, IEquatable<Distance>
    {
        private const int KilometersToMeters = 1000;
        private const float YardsToMeters = 0.9144f;
        private const float MilesToMeters = 1609.34f;

        public double DistanceInMeters { get; private set; }

        public double DistanceInKilometers
        {
            get
            {
                return DistanceInMeters / KilometersToMeters;
            }
        }

        public double DistanceInMiles
        {
            get
            {
                return DistanceInMeters / MilesToMeters;
            }
        }

        public double DistanceInYards
        {
            get
            {
                return DistanceInMeters / YardsToMeters;
            }
        }

        public static Distance Zero = Distance.FromMeters(0);

        private Distance(double distanceInMeters)
        {
            //Allow negative distance
            this.DistanceInMeters = distanceInMeters;

            //if (distanceInMeters >= 0)
            //{
            //    this.DistanceInMeters = distanceInMeters;
            //}
            //else
            //{
            //    throw new ArgumentException("Distance must be greater than or equal to zero.");
            //}
        }

        public static Distance FromMeters(double meters)
        {
            return new Distance(meters);
        }

        public static Distance FromYards(double yards)
        {
            return new Distance(yards * YardsToMeters);
        }

        public static Distance FromMiles(double miles)
        {
            return new Distance(miles * MilesToMeters);
        }

        public int CompareTo(Distance other)
        {
            return DistanceInMeters.CompareTo(other.DistanceInMeters);
        }

        public static Distance operator +(Distance a, Distance b)
        {
            return new Distance(a.DistanceInMeters + b.DistanceInMeters);
        }

        public static Distance operator *(Distance a, double value)
        {
            return new Distance(a.DistanceInMeters * value);
        }

        public static Distance operator *(Distance a, Distance b)
        {
            return new Distance(a.DistanceInMeters * b.DistanceInMeters);
        }

        public static Distance Add(Distance a, Distance b)
        {
            return new Distance(a.DistanceInMeters + b.DistanceInMeters);
        }

        public static Distance operator -(Distance a, Distance b)
        {
            return new Distance(a.DistanceInMeters - b.DistanceInMeters);
        }

        public static Distance Subtract(Distance a, Distance b)
        {
            return new Distance(a.DistanceInMeters - b.DistanceInMeters);
        }

        public static bool operator >(Distance a, Distance b)
        {
            return a.DistanceInMeters > b.DistanceInMeters;
        }

        public static bool operator <(Distance a, Distance b)
        {
            return a.DistanceInMeters < b.DistanceInMeters;
        }

        public static bool operator >=(Distance a, Distance b)
        {
            return a.DistanceInMeters >= b.DistanceInMeters;
        }

        public static bool operator <=(Distance a, Distance b)
        {
            return a.DistanceInMeters <= b.DistanceInMeters;
        }

        public static bool operator ==(Distance a, Distance b)
        {
            if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
            {
                return Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null);
            }

            return a.DistanceInMeters == b.DistanceInMeters;
        }

        public static bool operator !=(Distance a, Distance b)
        {
            return !(a == b);
        }  

        public override bool Equals(object obj)
        {
            var other = obj as Distance;

            if (other != null)
            {
                return DistanceInMeters == other.DistanceInMeters;
            }

            return false;
        }

        public bool Equals(Distance other)
        {
            return DistanceInMeters == other.DistanceInMeters;
        }

        public override int GetHashCode()
        {
            return DistanceInMeters.GetHashCode();
        }
    }
}
