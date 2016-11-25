using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Config;

namespace Snappy.MapMatching
{
    /// <summary>
    /// Keeps track of the location of a vehicle in a road network via a probabilistic model. 
    /// Its state encodes the proability of being on a given (directed) road in the network.
    /// The state can be updated by inputting co-ordinates.
    /// </summary>
    public class MapMatcher
    {
        public RoadGraph Graph { get; set; }

        public RoadSearchGrid SearchGrid { get; set; }

        public MapMatchState State { get; set; }

        public MapMatcher(RoadGraph graph)
        {
            SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, Constants.Search_Grid_Grid_Size_In_Meters);
            Graph = graph;
            State = MapMatchState.InitialState();
        }

        public bool TryUpdateState(Coord coord, out UpdateAnalytics analytics, DateTime timeStamp = default(DateTime), bool printUpdateAnalyticsToConsole = false)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();


            analytics = new UpdateAnalytics();
            analytics.Coordinate = coord;
            analytics.PrevProbabilityVector = State.Probabilities;

            // Find nearby roads using search grid
            List<DirectedRoad> nearbyRoads = SearchGrid.GetNearbyValues(coord, Constants.Search_Grid_Grid_Size_In_Meters);
            if(nearbyRoads.Count == 0)
            {
                // If no nearby roads, update fails. 
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoNearbyRoads;
                return false;
            }

            // Project coordinate onto roads 
            List<ProjectToRoad> nearbyRoadProjections = nearbyRoads.Select(x => new ProjectToRoad(coord, x)).ToList();

            // Initialize new transition memory 
            var transitionMemory = new Dictionary<DirectedRoad, DirectedRoad>();
            // Initialize new probability vector 
            var newProbabilityVector = new ProbabilityVector<DirectedRoad>();

            if(State.PrevCoord == null)
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection);
                    analytics.Emissions[projection.Road] = emission;
                    newProbabilityVector[projection.Road] = emission.Probability;
                    transitionMemory[projection.Road] = null;
                }
            }
            else
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    //Calculate emission probability
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection);
                    analytics.Emissions[projection.Road] = emission;

                    //Calculate maximum transition from possible prev state 
                    var maxCandidates = new Dictionary<Transition, double>();
                    analytics.AllTransitions[projection.Road] = new List<Transition>();
                    foreach (var prevProjection in State.PrevNearbyRoadsAndProjections)
                    {
                        Transition transition = MarkovProbabilityHelpers.TransitionProbability(Graph, prevProjection, projection);
                        analytics.AllTransitions[projection.Road].Add(transition);
                        maxCandidates[transition] = transition.Probability * State.Probabilities[prevProjection.Road];
                    }
                    var maxPair = maxCandidates.Aggregate((x, y) => x.Value > y.Value ? x : y);

                    analytics.MaxTransitions[projection.Road] = maxPair.Key;

                    //probability update
                    newProbabilityVector[projection.Road] = maxPair.Value * emission.Probability;

                    //transition memory 
                    transitionMemory[projection.Road] = maxPair.Key.From;
                }
            }

            // CHECK IF UPDATE FAILED
            if(analytics.Emissions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.ZeroEmissions;
                return false;
            }            
            if( analytics.MaxTransitions.Count > 0 && analytics.MaxTransitions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                return false;
            }


            if(newProbabilityVector.GetSum() < double.Epsilon)
            {
                throw new Exception("SUM TING WONG");
            }
            analytics.NonNormalizedProbabilityVector = newProbabilityVector;

            //UPDATE STATE: 

            // 1. Update probability vector
            newProbabilityVector =  newProbabilityVector.Normalize();
            State.Probabilities = newProbabilityVector;

            //2. Add to transition memory 
            State.TransitionMemory.Add(transitionMemory);

            //3. Update prev coord and date 
            State.PrevCoord = coord;
            State.PrevTime = timeStamp;            

            // 4. 
            State.PrevNearbyRoadsAndProjections = nearbyRoadProjections;

            analytics.ProbabilityVector = State.Probabilities;
            analytics.UpdateStatus = Enums.MapMatchUpdateStatus.SuccessfullyUpdated;
            stopwatch.Stop();
            analytics.UpdateTimeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            //Console.WriteLine(analytics.BuildSummaryString(5));

            if (printUpdateAnalyticsToConsole)
            {
                analytics.PrintUpdateSummary(20);
            }
            return true;
        }
        public void Reset()
        {
            State.Reset();
        }
    }
}
