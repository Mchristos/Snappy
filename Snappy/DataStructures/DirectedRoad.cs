using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;

namespace Snappy.DataStructures
{
    public class DirectedRoad : IEquatable<DirectedRoad>
    {
        public long Start { get; set; }

        public long End { get; set; }

        public List<Coord> Geometry { get; set; }

        public string Name { get; set; }

        public string Squid { get; set; }

        public DirectedRoad(long startNodeId, long endNodeId, List<Coord> geometry, string name)
        {
            Start = startNodeId;
            End = endNodeId;
            Geometry = geometry;
            Name = name;
            Squid = Extensions.GetNewSquid();
        }

        private Distance _length { get; set; }

        public Distance Length
        {
            get
            {
                if (_length == null)
                {
                    _length = Geometry.ComputeLength();
                }
                return _length;
            }
        }

        public override int GetHashCode()
        {
            return Squid.GetHashCode();
        }

        public bool Equals(DirectedRoad other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End) && GetHashCode() == other.GetHashCode();
        }
    }
}