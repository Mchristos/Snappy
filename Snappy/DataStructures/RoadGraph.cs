using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.DataStructures
{
    public class RoadGraph : Dictionary<long, List<DirectedRoad>>
    {
        public RoadGraph() : base()
        {
            _nodeLookup = new Dictionary<long, Coord>();
            _roadLookup = new Dictionary<string, DirectedRoad>();
        }

        public void AddRoad(DirectedRoad road)
        {
            if (this.ContainsKey(road.Start))
            {
                //the beginning of the road ia alreasy a node in the graph
                this[road.Start].Add(road);
            }
            else
            {
                // add the start of the road as a node in the graph, plus the edge. 
                this[road.Start] = new List<DirectedRoad>() { road };
                _nodeLookup[road.Start] = road.Geometry.First();
                _roadLookup[road.Squid] = road;                
             }
        }


        private Dictionary<long, Coord> _nodeLookup { get; set; }
        private Dictionary<string, DirectedRoad> _roadLookup { get; set; }



        public List<Coord> Nodes
        {
            get
            {
                return _nodeLookup.Values.ToList();
            }
        }
        public List<DirectedRoad> Roads
        {
            get
            {
                return _roadLookup.Values.ToList();
            }
        }


    }
}
