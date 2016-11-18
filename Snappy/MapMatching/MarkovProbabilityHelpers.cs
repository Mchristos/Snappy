﻿using Snappy.Config;
using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;

namespace Snappy.MapMatching
{
    internal class MarkovProbabilityHelpers
    {
        public static Emission EmissionProbability(ProjectToRoad projection)
        {
            return new Emission(projection);
        }

        public static Transition TransitionProbability(RoadGraph graph, ProjectToRoad projection1, ProjectToRoad projection2)
        {
            DirectedRoad road1 = projection1.Road;
            DirectedRoad road2 = projection2.Road;

            //calculate on road distance
            double onRoadDistanceInMeters;
            Distance startingDist = projection1.DistanceToEnd;
            Distance endDist = projection2.DistanceFromStart;

            // Roads are the same:
            if (road1.Equals(road2))
            {
                //negative if going backwards along road
                onRoadDistanceInMeters = projection2.DistanceFromStart.DistanceInMeters - projection1.DistanceFromStart.DistanceInMeters;
            }

            // Roads are connected
            else if (road1.End == road2.Start)
            {
                onRoadDistanceInMeters = startingDist.DistanceInMeters + endDist.DistanceInMeters;
            }

            // Try connect roads using Dijstra
            else
            {
                List<DirectedRoad> path;
                if (PathFinding.DijstraTryFindPath(graph, road1.End, road2.Start, out path))
                {
                    Distance connectingDist = Distance.Zero;
                    foreach (var road in path)
                    {
                        connectingDist += road.Length;
                    }
                    onRoadDistanceInMeters = startingDist.DistanceInMeters + connectingDist.DistanceInMeters + endDist.DistanceInMeters;
                }
                else
                {
                    //cannot connect up roads. transition probability is zero
                    return Transition.ImpossibleTransition(road1, road2);
                }
            }

            Distance haversineDistance = projection1.Coordinate.HaversineDistance(projection2.Coordinate);

            return new Transition(onRoadDistanceInMeters, haversineDistance.DistanceInMeters, road1, road2);
        }
    }
}