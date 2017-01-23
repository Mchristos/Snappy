using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snappy.DataStructures
{
    [DebuggerDisplay("{Name} {Start} --> {End}")]
    public class DirectedRoad : IEquatable<DirectedRoad>
    {
        public string Start { get; set; }

        public string End { get; set; }

        public List<Coord> Geometry { get; set; }

        public string Name { get; set; }

        public string Squid { get; set; }

        public DirectedRoad(string startNodeId, string endNodeId, List<Coord> geometry, string name)
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