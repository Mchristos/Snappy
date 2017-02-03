using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Snappy.Functions;
using Snappy.MapMatching;
using Xunit;
using Snappy.OpenStreetMaps;
using Snappy.ValueObjects;

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
            var mapmatcher = new OsmMapMatcher(roadgraph);

            foreach (var coord in coords)
            {
                // If TryUpdateState returns true, the UpdateStatus in UpdateAnalytics must be true
                // Conversely if TryUpdateState returns false UpdateStatus must not be true.
                if (mapmatcher.TryUpdateState(coord))
                {
                    Assert.Equal(Enums.MapMatchUpdateStatus.SuccessfullyUpdated, mapmatcher.LastUpdateAnalytics.UpdateStatus);
                }
                else
                {
                    Assert.False(Enums.MapMatchUpdateStatus.SuccessfullyUpdated == mapmatcher.LastUpdateAnalytics.UpdateStatus);
                }
                var analytics = mapmatcher.LastUpdateAnalytics;

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

        // TO DO : Complete this test 
        [Fact]
        public void TestGenericMapMatcher()
        {
            var AB = new CustomRoad(Geometry.AB.ToCoordList(), "ab");
            var BC = new CustomRoad(Geometry.BC.ToCoordList(), "bc");
            var BD = new CustomRoad(Geometry.BD.ToCoordList(), "bd");
            var customRoads = new List<CustomRoad>()
            {
                AB,
                BC,
                BD
            };
            var mapmatcher = new MapMatcher<CustomRoad>(customRoads, x => new DataStructures.DirectedRoad( getIdTupleFromName(x.Name).Item1, getIdTupleFromName(x.Name).Item2, x.Geometry, x.Name), MapMatcherParameters.Default);
            var fakeTrack = Geometry.FakeVehicleTrackABD.ToCoordList();

            foreach (var point in fakeTrack)
            {
                bool updated = mapmatcher.TryUpdateState(point);
                Assert.Equal(true, updated);
            }
            var sequence = mapmatcher.GetMostLikelySequence().Select(x => x.Datum.Name).ToList();

            // Check that the obtained most likely sequence matches with the expected sequence
            Assert.Equal(Results.CorrectSequenceOfRoadNames, sequence.ToArray());
        }

        private Tuple<string, string> getIdTupleFromName(string name)
        {
            // takes name, e.g. "ab", and gets two ids, "a" and "b" 
            var split = name.ToArray();
            string first = split.First().ToString();
            string last = split.Last().ToString();
            return new Tuple<string, string>(first, last);
        }

    }


    public class CustomRoad
    {
        public List<Coord> Geometry { get; set; }
        public string Name { get; set; }

        public CustomRoad(List<Coord> geometry, string name)
        {
            Geometry = geometry;
            Name = name;
        }
    }

    public static class Results
    {
        public static string[] CorrectSequenceOfRoadNames
        {
            get
            {
                return new string[]
                {
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "ab",
                    "bd",
                    "bd",
                    "bd",
                    "bd",
                    "bd",
                    "bd",
                    "bd",
                    "bd",
                };
            }
        }

    }




    public static class Geometry
    {
        public static double[,] AB
        {
            get
            {
                return new double[,]
                {
                     { -34.3107596046969, 18.411111831665 },
                     { -34.3038115581275, 18.4108114242554 }
                };
            }
        }
        public static double[,] BC
        {
            get
            {
                return new double[,]
                {
                    { -34.3038115581275, 18.4108114242554 },
                     { -34.3001246060852, 18.4033870697021 }
                };
            }
        }
        public static double[,] BD
        {
            get
            {
                return new double[,]
                {
                     { -34.3038115581275, 18.4108114242554 },
                     { -34.2993801057438, 18.4168195724487 }
                };
            }
        }

        public static double[,] FakeVehicleTrackABD
        {
            get
            {
                return new double[,]
                {
                     { -34.3105469179242, 18.4114336967468 },
                     { -34.3097847858978, 18.4114336967468 },
                     { -34.3094303035283, 18.4114336967468 },
                     { -34.3089694742106, 18.4113693237305 },
                     { -34.3084554692955, 18.4111976623535 },
                     { -34.3079769101667, 18.4111332893372 },
                     { -34.3074097254451, 18.410918712616 },
                     { -34.3069843343897, 18.410918712616 },
                     { -34.3065589411793, 18.4108757972717 },
                     { -34.3059740219962, 18.4108757972717 },
                     { -34.3053004736475, 18.4107685089111 },
                     { -34.3047332708478, 18.4107685089111 },
                     { -34.3037938327837, 18.4112191200256 },
                     { -34.3032620707311, 18.4117770195007 },
                     { -34.3030139139543, 18.4122276306152 },
                     { -34.3024112444435, 18.4136867523193 },
                     { -34.3015426837174, 18.4146738052368 },
                     { -34.3009577295953, 18.4153389930725 },
                     { -34.3004614016908, 18.4160041809082 },
                     { -34.3002664149259, 18.4161972999573 }
                };
            }
        }



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
