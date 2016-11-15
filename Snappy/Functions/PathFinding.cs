using Snappy.DataStructures;
using System.Collections.Generic;
using System;
using Sys = System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{

    public class NodeMemory
    {
        public double Distance { get; set; }

        public long Prev { get; set; }
    }
    public static class PathFinding
    {

        /// <summary>
        /// Finds the shortest sequence of directed roads from the origin to destination node using Dijstra's algorithm.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static List<DirectedRoad> DijstraFindPath(RoadGraph graph, long origin, long destination)
        {

            if (!graph.ContainsKey(origin))
            {
                throw new ArgumentException("graph does not contain origin as a node");
            }
            if (!graph.ContainsKey(destination))
            {
                //graph does not necessarily contain destination as a key...
            }
            if (origin == destination) { return new List<DirectedRoad>(); }






            var dist = new Dictionary<long, NodeMemory>();

            var dist = new Dictionary<long, double>();
            // for each node gives the directed road that it CAME FROM
            var prev = new Dictionary<long, DirectedRoad>();
            dist[origin] = 0;


            var Q = new HashSet<long>();
            Q.Add(origin);
            long currentNode = origin;


            while (Q.Count != 0)
            {
                currentNode = Q.Aggregate((x, y) => dist[x] < dist[y] ? x : y);

                if (currentNode == destination)
                {
                    break;
                }
                if (graph.ContainsKey(currentNode))
                {
                    foreach (var road in graph[currentNode])
                    {
                        var alt = dist[currentNode] + road.Length.DistanceInMeters;

                        if (dist.ContainsKey(road.End))
                        {
                            if (alt < dist[road.End])
                            {
                                dist[road.End] = alt;
                                prev[road.End] = road;
                                Q.Add(road.End);
                            }
                        }
                        else
                        {
                            dist[road.End] = alt;
                            prev[road.End] = road;
                            Q.Add(road.End);
                        }
                    }
                }
                Q.Remove(currentNode);
            }

            //trace back path

            List<DirectedRoad> result = new List<DirectedRoad>();
            long tracer = destination;
            while (tracer != origin)
            {
                var road = prev[tracer];
                result.Add(road);
                tracer = road.Start;
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Connects gaps and removes duplicates in a sequence of roads in a graph. 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static List<DirectedRoad> DijstraConnectUpSequence(this List<DirectedRoad> sequence, RoadGraph graph)
        {
            if (sequence.Count == 0) { throw new System.ArgumentException("Empty sequence"); }
            if (sequence.Count == 1) { return sequence; }


            var cleanedSequence = sequence.RemoveDuplicateNeighbors();
            var result = new List<DirectedRoad>() { cleanedSequence.First() };
            for (int i = 1; i < cleanedSequence.Count; i++)
            {
                //add all necessary roads up to and including the i'th
                List<DirectedRoad> connection = DijstraFindPath(graph, cleanedSequence[i - 1].End, cleanedSequence[i].Start);
                result.AddRange(connection);
                result.Add(cleanedSequence[i]);
            }
            return result;
        }





















        //public static List<DirectedRoad> DijstraConnectRoads(this RoadGraph graph, DirectedRoad start, DirectedRoad end)
        //{
        //    //Djikstra <3
        //    var openList = new C5.IntervalHeap<GraphSearch>();
        //    var closedList = new Sys.HashSet<long>();
        //    openList.Add(new GraphSearch(start, null, 0, start.Length.DistanceInMeters));
        //    var currentItem = openList.FindMin();
        //    openList.DeleteMin();

        //    do
        //    {
        //        closedList.Add(currentItem.Edge.End);

        //        var next = graph[currentItem.Edge.End];
        //        if (currentItem.Depth < 1) //Depth setting
        //        {
        //            foreach (var neighbourEdge in next)
        //            {
        //                if (!closedList.Contains(neighbourEdge.End))
        //                {
        //                    openList.Add(new GraphSearch(
        //                        neighbourEdge,
        //                        currentItem,
        //                        currentItem.Depth + 1,
        //                        currentItem.Weight + neighbourEdge.Length.DistanceInMeters));
        //                }
        //            }
        //        }
        //        if (openList.IsEmpty)
        //        {
        //            break;
        //        }
        //        currentItem = openList.FindMin();
        //        openList.DeleteMin();
        //        if (currentItem.Edge.End == end.Start)
        //        {
        //            var result = GetResult(currentItem);
        //            return result;
        //        }
        //    } while (!openList.IsEmpty);
        //    return new List<DirectedRoad>();
        //    //throw new System.Exception("Failed to find connection between two roads. Perhaps they are not connected?");
        //}

        //private static List<DirectedRoad> GetResult(GraphSearch currentItem)
        //{
        //    var results = new List<DirectedRoad>();

        //    while (currentItem.Previous != null)
        //    {
        //        results.Add(currentItem.Edge);
        //        currentItem = currentItem.Previous;
        //    }

        //    results.Reverse();
        //    return results;
        //}

    }








}