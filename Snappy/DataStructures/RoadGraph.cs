using Snappy.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.DataStructures
{
    public class RoadGraph : Dictionary<string, List<DirectedRoad>>
    {
        public RoadGraph() : base()
        {
            _nodeLookup = new Dictionary<string, Coord>();
            _roadLookup = new Dictionary<string, DirectedRoad>();
            _inverseGraph = new Dictionary<string, List<DirectedRoad>>();
        }

        public void AddRoad(DirectedRoad road)
        {
            _nodeLookup[road.Start] = road.Geometry.First();
            _roadLookup[road.Squid] = road;
            if (this.ContainsKey(road.Start))
            {
                //the beginning of the road ia alreasy a node in the graph
                this[road.Start].Add(road);
            }
            else
            {
                // add the start of the road as a node in the graph, plus the edge.
                this[road.Start] = new List<DirectedRoad>() { road };
            }
            if (_inverseGraph.ContainsKey(road.End))
            {
                _inverseGraph[road.End].Add(road);
            }
            else
            {
                _inverseGraph[road.End] = new List<DirectedRoad>() { road };
            }
        }

        // Look up node position by id
        private Dictionary<string, Coord> _nodeLookup { get; set; }

        // Look up road by squid
        private Dictionary<string, DirectedRoad> _roadLookup { get; set; }

        // Look up number of incoming roads to given node
        private Dictionary<string, List<DirectedRoad>> _inverseGraph { get; set; }

        public Dictionary<string, Coord> Nodes
        {
            get
            {
                return _nodeLookup;
            }
        }

        public List<DirectedRoad> Roads
        {
            get
            {
                return _roadLookup.Values.ToList();
            }
        }

        // Stores INCOMING roads to a given node
        public Dictionary<string, List<DirectedRoad>> InverseGraph
        {
            get
            {
                return _inverseGraph;
            }
        }
    }
}