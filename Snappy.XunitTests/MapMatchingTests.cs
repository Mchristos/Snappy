using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Snappy.Functions;
using Snappy.MapMatching;
using Xunit;

namespace Snappy.XunitTests
{
    public class MapMatchingTests
    {
        [Fact]
        public void TestThatMapMatchingUpdateAnalyticsAreRecordedCorrectly()
        {
            var coords = Geometry.GeometryAlongVikingWay.ToCoordList();
            var boundingBox = coords.GetBoundingBox();
            var roadgraph = OpenStreetMaps.OsmGraphBuilder.BuildInRegion(Config.Urls.MainOverpassApi, boundingBox);
            var mapmatcher = new MapMatcher(roadgraph);

            var analytics = new UpdateAnalytics();
            foreach (var coord in coords)
            {
                // If TryUpdateState returns true, the UpdateStatus in UpdateAnalytics must be true
                // Conversely if TryUpdateState returns false UpdateStatus must not be true.
                if (mapmatcher.TryUpdateState(coord, out analytics))
                {
                    Assert.Equal(Enums.MapMatchUpdateStatus.SuccessfullyUpdated, analytics.UpdateStatus);
                }
                else
                {
                    Assert.False(Enums.MapMatchUpdateStatus.SuccessfullyUpdated == analytics.UpdateStatus);
                }

                // The probability vector in the analytcs object must be equal to the probability vector in the map matcher state. 
                Assert.Equal(mapmatcher.State.Probabilities, analytics.ProbabilityVector);

                // Each transition "remembered" by roads must transition "to" the road in question
                foreach (var pair in analytics.MaxTransitions)
                {
                    Assert.Equal(pair.Key, pair.Value.To);
                }

                // Check that newProbability = probOfTranferredFrom * transitionProb * emissionProb
                // for each nearby road at this step (before normalization) 
                if(analytics.MaxTransitions.Count > 0)
                {
                    foreach (var road in mapmatcher.State.Probabilities.Keys)
                    {

                        var emissionProb = analytics.Emissions[road].Probability;
                        Transition transition = analytics.MaxTransitions[road];
                        var transitionProb = transition.Probability;
                        var prevProbabilityOfFrom = analytics.PrevProbabilityVector[transition.From];
                        double expectedNonNormalizedProbability = prevProbabilityOfFrom * transitionProb * emissionProb;

                        Assert.Equal(expectedNonNormalizedProbability, analytics.NonNormalizedProbabilityVector[road], 8);
                    }
                }



            }



        }



    }

    public static class Geometry
    {
        public static double[,] GeometryAlongVikingWay
        {
            get
            {
                return new double[,]
                {       
                 { -33.9269101695321, 18.5472822189331 },
                 { -33.9267143197131, 18.5496211051941 },
                 { -33.9270169965162, 18.5517883300781 },
                 { -33.9268033424139, 18.5534405708313 },
                 { -33.9269279740387, 18.5555648803711 }
                };
            }

        }
    }

}
