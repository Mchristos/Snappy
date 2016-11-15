using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.DataStructures
{
    public class GraphSearch : IComparable<GraphSearch>
    {
        public DirectedRoad Edge { get; }

        public GraphSearch Previous { get; }

        public int Depth { get; }

        public double Weight { get; }

        public GraphSearch(DirectedRoad edge, GraphSearch previous, int depth, double weight)
        {
            Previous = previous;
            Edge = edge;
            Depth = depth;
            Weight = weight;
        }

        public int CompareTo(GraphSearch other)
        {
            return Weight.CompareTo(other.Weight);
        }

        public bool Equals(GraphSearch other)
        {
            return Edge.Equals(other.Edge);
        }

        public override int GetHashCode()
        {
            return Edge.GetHashCode();
        }
    }
}
