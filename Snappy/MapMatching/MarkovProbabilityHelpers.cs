using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.MapMatching
{
    class MarkovProbabilityHelpers
    {

        public static double EmissionProbability(ProjectToRoad projection)
        {
            return ProbabilityFunctions.HalfGaussian(projection.ProjectedDistance.DistanceInMeters, Constants.Sigma_Value_In_Meters_For_Emissions);
        }

        public static double TransitionProbability(RoadGraph graph, ProjectToRoad projection1, ProjectToRoad projection2)
        {
            //calculate on road distance 
            Distance onRoadDistance;
            Distance startingDist = projection1.DistanceToEnd;
            Distance endDist = projection2.DistanceFromStart;

            if (projection1.Road == projection2.Road)
            {
                //negative if going backwards along road
                onRoadDistance = projection2.DistanceFromStart - projection1.DistanceFromStart;
            }
            else if( projection1.Road.End == projection2.Road.Start)
            {
                //CHANGE
                onRoadDistance = projection2.DistanceFromStart - projection1.DistanceFromStart;
            }
            else
            {
                var connection = PathFinding.DijstraConnectRoads(graph, projection1.Road, projection2.Road);

                if(connection.Count > 0)
                {
                    Distance middleDist = connection.Select(x => x.Length).Aggregate((x, y) => x + y);

                    onRoadDistance = startingDist + middleDist + endDist;
                }
                else
                {
                    onRoadDistance = startingDist + endDist;
                }

            }



            Distance haversineDistance = projection1.Coordinate.HaversineDistance(projection2.Coordinate);

            double diffInMeters = Math.Abs(haversineDistance.DistanceInMeters - onRoadDistance.DistanceInMeters);
            return ProbabilityFunctions.ExponentialDistribution(diffInMeters, Constants.Beta_For_Transitions_In_Meters);

        }



    }
}
