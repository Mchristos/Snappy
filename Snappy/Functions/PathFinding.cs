using C5;
using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class PathFinding
    {
        /// <summary>
        /// Finds the shortest sequence of directed roads from the origin to destination node using Dijstra's algorithm.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static bool DijstraTryFindPath(RoadGraph graph, long origin, long destination, out List<DirectedRoad> path)
        {
            path = new List<DirectedRoad>();
            if (!graph.ContainsKey(origin))
            {
                return false;
            }
            if (origin == destination)
            {
                // correct path is empty
                return true;
            }

            DijstraSearchItem currentSearch = new DijstraSearchItem(origin, null, null, 0);

            IntervalHeap<DijstraSearchItem> heap = new IntervalHeap<DijstraSearchItem>();
            var itemLookup = new Dictionary<long, DijstraSearchItem>();
            heap.Add(currentSearch);
            itemLookup[origin] = currentSearch;

            while (heap.Count != 0)
            {
                // find the least item and remove from heap
                currentSearch = heap.FindMin();
                if (currentSearch.Distance > Config.Constants.Dijstra_Upper_Search_Limit_In_Meters)
                {
                    return false;
                }
                heap.DeleteMin();

                if (currentSearch.Id == destination)
                {
                    break;
                }

                if (graph.ContainsKey(currentSearch.Id))
                {
                    foreach (var road in graph[currentSearch.Id])
                    {
                        // calculate new possible distance
                        var alt = currentSearch.Distance + road.Length.DistanceInMeters;
                        var potentialNewItem = new DijstraSearchItem(road.End, road, currentSearch, alt);

                        if (itemLookup.ContainsKey(road.End))
                        {
                            DijstraSearchItem searchItem = itemLookup[road.End];
                            if (alt < searchItem.Distance)
                            {
                                itemLookup[road.End] = potentialNewItem;
                                heap.Add(potentialNewItem);
                            }
                        }
                        else
                        {
                            itemLookup[road.End] = potentialNewItem;
                            heap.Add(potentialNewItem);
                        }
                    }
                }
            }

            //trace back path
            List<DirectedRoad> result = new List<DirectedRoad>();
            DijstraSearchItem tracer = currentSearch;
            while (tracer.PrevRoad != null)
            {
                result.Add(tracer.PrevRoad);
                tracer = tracer.Prev;
            }
            result.Reverse();
            path = result;
            return true;
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
                List<DirectedRoad> connection;

                if (DijstraTryFindPath(graph, cleanedSequence[i - 1].End, cleanedSequence[i].Start, out connection))
                {
                    result.AddRange(connection);
                    result.Add(cleanedSequence[i]);
                }
                else
                {
                    throw new Exception("Unable to connect up sequence of roads");
                }
            }
            return result;
        }








        //Jan's beastly dijstra 



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