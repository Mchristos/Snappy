using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System.Collections.Generic;

namespace Snappy.MapMatching
{
    internal class MarkovProbabilityHelpers
    {
        public static Emission EmissionProbability(ProjectToRoad projection, double sigmaValue)
        {
            return new Emission(projection, sigmaValue);
        }

        public static Transition TransitionProbability(RoadGraph graph, ProjectToRoad projection1, ProjectToRoad projection2, Parameters parameters)
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
            // Road start or end on the same node
            else if (road1.End == road2.End || road1.Start == road2.Start)
            {
                //make this transition impossible
                return Transition.ImpossibleTransition(road1, road2);
            }

            // Roads are connected (can be same road in opposite direction)
            else if (road1.End == road2.Start)
            {
                onRoadDistanceInMeters = startingDist.DistanceInMeters + endDist.DistanceInMeters;
            }

            // Try connect roads using Dijstra
            else
            {
                List<DirectedRoad> path;
                if (PathFinding.DijstraTryFindPath(graph, road1.End, road2.Start, parameters.DijstraUpperSearchLimit, out path))
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

            return new Transition(onRoadDistanceInMeters, haversineDistance.DistanceInMeters, road1, road2, parameters);
        }
    }
}