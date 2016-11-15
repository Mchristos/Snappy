﻿using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.OpenStreetMaps
{
    public static class OsmGraphBuilder
    {
        public static RoadGraph BuildInRegion(string apiUrl, BoundingBox boundingBox)
        {
            string response = OsmHelpers.GetOsmResponse(apiUrl, boundingBox);
            var elements = OsmHelpers.GetOsmElements(response);

            List<Way> ways = elements.Where(x => x is Way).Select(x => x as Way).ToList();
            var nodeLookup = elements.Where(x => x is OsmNode).Select(x => x as OsmNode).ToDictionary(x => x.Id, y => y);

            var intersectionCounter = ways.Select(x => x.Nodes).CountRepeatedIds();
            // build graph
            RoadGraph graph = new RoadGraph();
            foreach (var way in ways)
            {
                string wayName = way.ParseName();
                var subways = way.Nodes.InclusivePartitioning(id => intersectionCounter[id] > 1);
                foreach (var subway in subways)
                {
                    var roadShape = subway.Select(id => nodeLookup[id].ToCoord()).ToList();
                    DirectedRoad road = new DirectedRoad(subway.First(), subway.Last(), roadShape, wayName);
                    graph.AddRoad(road);
                    if (!way.IsOneWay())
                    {
                        DirectedRoad reverseRoad = road.Reverse();
                        graph.AddRoad(reverseRoad);
                    }
                }
            }
            return graph;
        }
    }
}