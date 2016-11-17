using Snappy.Config;
using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;

namespace Snappy.MapMatching
{
    internal class MarkovProbabilityHelpers
    {
        public static double EmissionProbability(ProjectToRoad projection)
        {
            return ProbabilityFunctions.HalfGaussian(projection.ProjectedDistance.DistanceInMeters, Constants.Sigma_Value_In_Meters_For_Emissions);
        }

        public static double TransitionProbability(RoadGraph graph, ProjectToRoad projection1, ProjectToRoad projection2)
        {
            DirectedRoad road1 = projection1.Road;
            DirectedRoad road2 = projection2.Road;

            //calculate on road distance
            Distance onRoadDistance;
            Distance startingDist = projection1.DistanceToEnd;
            Distance endDist = projection2.DistanceFromStart;

            // Roads are the same:
            if (road1.Equals(road2))
            {
                //negative if going backwards along road
                onRoadDistance = projection2.DistanceFromStart - projection1.DistanceFromStart;
            }

            // Roads are connected
            else if (road1.End == road2.Start)
            {
                onRoadDistance = startingDist + endDist;
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
                    onRoadDistance = startingDist + connectingDist + endDist;
                }
                else
                {
                    //cannot connect up roads. transition probability is zero
                    return 0;
                }
            }

            Distance haversineDistance = projection1.Coordinate.HaversineDistance(projection2.Coordinate);

            double diffInMeters = Math.Abs(haversineDistance.DistanceInMeters - onRoadDistance.DistanceInMeters);
            return ProbabilityFunctions.ExponentialDistribution(diffInMeters, Constants.Beta_For_Transitions_In_Meters);
        }
    }
}