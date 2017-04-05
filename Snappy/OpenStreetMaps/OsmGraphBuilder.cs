using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.OpenStreetMaps
{
    public static class OsmGraphBuilder
    {
        public static RoadGraph BuildInRegion(string apiUrl, BoundingBox boundingBox, bool highwayTags = true, bool railTags = true, bool ignoreOneWAys = false)
        {
            return BuildInRegion(apiUrl, new List<BoundingBox>() { boundingBox }, highwayTags, railTags, ignoreOneWAys); 
        }
        public static RoadGraph BuildInRegion(string apiUrl, List<BoundingBox> boundingBoxes, bool highwayTags = true, bool railTags = true, bool ignoreOneWays = false)
        {
            var allOsmElements = new List<Element>();
            foreach (var box in boundingBoxes)
            {
                string response = OsmHelpers.GetOsmResponse(apiUrl, box, highwayTags, railTags);
                var elements = OsmHelpers.GetOsmElements(response);
                allOsmElements.AddRange(elements);
            }
            var graph = BuildGraph(allOsmElements, ignoreOneWays);
            return graph;
        }
        /// <summary>
        /// Build graph from OSM elements. May contains duplicated elements (from multiple calls) 
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private static RoadGraph BuildGraph(List<Element> elements, bool ignoreOneWays)
        {
            RoadGraph result = new RoadGraph();

            IEnumerable<Way> ways = elements.Where(x => x is Way).Select(x => x as Way).Distinct();
            var nodeLookup = new Dictionary<long, OsmNode>();
            foreach (var node in elements.Where(x => x is OsmNode).Select(x => x as OsmNode))
            {
                nodeLookup[node.Id] = node;
            }
            var intersectionCounter = ways.Select(x => x.Nodes).CountRepeatedIds();
            foreach (var way in ways)
            {
                string wayName = way.ParseName();
                var subways = way.Nodes.InclusivePartitioning(id => intersectionCounter[id] > 1);
                foreach (var subway in subways)
                {
                    var roadShape = subway.Select(id => nodeLookup[id].ToCoord()).ToList();
                    DirectedRoad road = new DirectedRoad(subway.First().ToString(), subway.Last().ToString(), roadShape, wayName);
                    result.AddRoad(road);
                    if (!way.IsOneWay() || ignoreOneWays)
                    {
                        DirectedRoad reverseRoad = road.Reverse();
                        result.AddRoad(reverseRoad);
                    }
                }
            }

            return result;
        }

    }
}