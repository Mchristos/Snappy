using Snappy.Config;
using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.MapMatching
{
    /// <summary>
    /// Keeps track of the location of a vehicle in a road network via a probabilistic model.
    /// Its state encodes the proability of being on a given (directed) road in the network.
    /// The state can be updated by inputting co-ordinates.
    /// </summary>
    public class MapMatcher<T>
    {
        public RoadGraph Graph { get; set; }

        public RoadSearchGrid SearchGrid { get; set; }

        public MapMatchState State { get; set; }

        public MapMatcherParameters Parameters { get; set; }

        public UpdateAnalytics LastUpdateAnalytics { get; set; }


        private Dictionary<string, T> _dataByRoadId { get; set; }


        public MapMatcher(List<T> data, Func<T, DirectedRoad> dataToRoad, MapMatcherParameters parameters, BoundingBox boundingBox = null)
        {
            // Initialize parameters
            Parameters = parameters;

            //Build graph
            _dataByRoadId = new Dictionary<string, T>();
            var graph = new RoadGraph();
            foreach (var datum in data)
            {
                var road = dataToRoad(datum);
                graph.AddRoad(road);
                _dataByRoadId[road.Squid] = datum;
            }
            Graph = graph;

            // Compute search grid (for accessing nearby roads)
            SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, parameters.NearbyRoadsThreshold, boundingBox);

            // Initialize state
            State = MapMatchState.InitialState();
        }
        public MapMatcher() { }
        public bool TryUpdateState(Coord coord, DateTime timeStamp = default(DateTime), bool printUpdateAnalyticsToConsole = false)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            LastUpdateAnalytics = new UpdateAnalytics();
            LastUpdateAnalytics.Coordinate = coord;
            LastUpdateAnalytics.PrevProbabilityVector = State.Probabilities;

            // Find nearby roads using search grid
            List<DirectedRoad> nearbyRoads = SearchGrid.GetNearbyValues(coord, Parameters.NearbyRoadsThreshold);
            if (nearbyRoads.Count == 0)
            {
                // If no nearby roads, update fails.
                LastUpdateAnalytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoNearbyRoads;
                stopwatch.Stop();
                LastUpdateAnalytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    LastUpdateAnalytics.PrintUpdateSummary();
                }

                return false;
            }

            // Project coordinate onto roads
            List<ProjectToRoad> nearbyRoadProjections = nearbyRoads.Select(x => new ProjectToRoad(coord, x)).ToList();

            // Initialize new transition memory
            var transitionMemory = new Dictionary<DirectedRoad, DirectedRoad>();
            // Initialize new probability vector
            var newProbabilityVector = new ProbabilityVector<DirectedRoad>();

            if (State.PrevCoord == null)
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection, Parameters.Sigma);
                    LastUpdateAnalytics.Emissions[projection.Road] = emission;
                    newProbabilityVector[projection.Road] = emission.Probability;
                    transitionMemory[projection.Road] = null;
                }
            }
            else
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    //Calculate emission probability
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection, Parameters.Sigma);
                    LastUpdateAnalytics.Emissions[projection.Road] = emission;

                    //Calculate maximum transition from possible prev state
                    var maxCandidates = new Dictionary<Transition, double>();
                    LastUpdateAnalytics.AllTransitions[projection.Road] = new List<Transition>();
                    foreach (var prevProjection in State.PrevNearbyRoadsAndProjections)
                    {
                        Transition transition = MarkovProbabilityHelpers.TransitionProbability(Graph, prevProjection, projection, Parameters);
                        LastUpdateAnalytics.AllTransitions[projection.Road].Add(transition);
                        maxCandidates[transition] = transition.Probability * State.Probabilities[prevProjection.Road];
                    }
                    var maxPair = maxCandidates.Aggregate((x, y) => x.Value > y.Value ? x : y);

                    LastUpdateAnalytics.MaxTransitions[projection.Road] = maxPair.Key;
                    LastUpdateAnalytics.PropogatedTransitionValues[maxPair.Key] = maxPair.Value;

                    //probability update
                    newProbabilityVector[projection.Road] = maxPair.Value * emission.Probability;

                    //transition memory
                    transitionMemory[projection.Road] = maxPair.Key.From;
                }
            }

            // CHECK IF UPDATE FAILED
            if (LastUpdateAnalytics.Emissions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                LastUpdateAnalytics.UpdateStatus = Enums.MapMatchUpdateStatus.ZeroEmissions;
                stopwatch.Stop();
                LastUpdateAnalytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    LastUpdateAnalytics.PrintUpdateSummary();
                }
                return false;
            }
            if (LastUpdateAnalytics.MaxTransitions.Count > 0 && LastUpdateAnalytics.MaxTransitions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                LastUpdateAnalytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                stopwatch.Stop();
                LastUpdateAnalytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    LastUpdateAnalytics.PrintUpdateSummary();
                }
                return false;
            }
            if (LastUpdateAnalytics.PropogatedTransitionValues.Count > 0 && LastUpdateAnalytics.PropogatedTransitionValues.Values.Sum() < double.Epsilon)
            {
                LastUpdateAnalytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                stopwatch.Stop();
                LastUpdateAnalytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    LastUpdateAnalytics.PrintUpdateSummary();
                }
                return false;
            }

            if (newProbabilityVector.GetSum() < double.Epsilon)
            {
                throw new Exception("New probability vector is zero everywhere. This problem should have been caught before this breakpoint is hit");
            }
            LastUpdateAnalytics.NonNormalizedProbabilityVector = newProbabilityVector;

            //UPDATE STATE:

            // 1. Update probability vector
            newProbabilityVector = newProbabilityVector.Normalize();
            State.Probabilities = newProbabilityVector;

            //2. Add to transition memory
            State.TransitionMemory.Add(transitionMemory);

            //3. Update prev coord and date
            State.PrevCoord = coord;
            State.PrevTime = timeStamp;

            // 4.
            State.PrevNearbyRoadsAndProjections = nearbyRoadProjections;

            LastUpdateAnalytics.ProbabilityVector = State.Probabilities;
            LastUpdateAnalytics.UpdateStatus = Enums.MapMatchUpdateStatus.SuccessfullyUpdated;
            stopwatch.Stop();
            LastUpdateAnalytics.UpdateTimeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            //Console.WriteLine(analytics.BuildSummaryString(5));

            if (printUpdateAnalyticsToConsole)
            {
                LastUpdateAnalytics.PrintUpdateSummary(20);
            }
            return true;
        }

        public void Reset()
        {
            State.Reset();
        }

        public List<MapState<T>> GetMostLikelySequence()
        {
            var result = new List<MapState<T>>();
            var roadSequence = State.GetMostLikelySequence();
            foreach (var road in roadSequence)
            {
                var mapState = new MapState<T>(road, _dataByRoadId[road.Squid]);
                result.Add(mapState);
            }
            return result;
        }
    }
}