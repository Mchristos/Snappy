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

        private Dictionary<string, T> _dataByRoadId { get; set; }

        public Parameters Parameters { get; set; }

        public MapMatcher(List<T> data, Func<T, DirectedRoad> dataToRoad, BoundingBox boundingBox = null, double dijstraUpperSearchLimit = DefaultValues.Dijstra_Upper_Search_Limit_In_Meters, double nearbyRoadRadius = DefaultValues.Nearby_Road_Radius_In_Meters)
        {
            // Initialize parameters
            Parameters = new Parameters( nearbyRoadsThreshold : nearbyRoadRadius,  dijstraUpperSearchLimit : dijstraUpperSearchLimit);

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
            SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, nearbyRoadRadius, boundingBox);

            // Initialize state
            State = MapMatchState.InitialState();
        }
        public MapMatcher() { }
        public bool TryUpdateState(Coord coord, out UpdateAnalytics analytics, DateTime timeStamp = default(DateTime), bool printUpdateAnalyticsToConsole = false)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            analytics = new UpdateAnalytics();
            analytics.Coordinate = coord;
            analytics.PrevProbabilityVector = State.Probabilities;

            // Find nearby roads using search grid
            List<DirectedRoad> nearbyRoads = SearchGrid.GetNearbyValues(coord, Parameters.NearbyRoadsThreshold);
            if (nearbyRoads.Count == 0)
            {
                // If no nearby roads, update fails.
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoNearbyRoads;
                stopwatch.Stop();
                analytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    analytics.PrintUpdateSummary();
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
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection, Parameters.Sigma);
                    analytics.Emissions[projection.Road] = emission;

                    //Calculate maximum transition from possible prev state
                    var maxCandidates = new Dictionary<Transition, double>();
                    analytics.AllTransitions[projection.Road] = new List<Transition>();
                    foreach (var prevProjection in State.PrevNearbyRoadsAndProjections)
                    {
                        Transition transition = MarkovProbabilityHelpers.TransitionProbability(Graph, prevProjection, projection, Parameters);
                        analytics.AllTransitions[projection.Road].Add(transition);
                        maxCandidates[transition] = transition.Probability * State.Probabilities[prevProjection.Road];
                    }
                    var maxPair = maxCandidates.Aggregate((x, y) => x.Value > y.Value ? x : y);

                    analytics.MaxTransitions[projection.Road] = maxPair.Key;
                    analytics.PropogatedTransitionValues[maxPair.Key] = maxPair.Value;

                    //probability update
                    newProbabilityVector[projection.Road] = maxPair.Value * emission.Probability;

                    //transition memory
                    transitionMemory[projection.Road] = maxPair.Key.From;
                }
            }

            // CHECK IF UPDATE FAILED
            if (analytics.Emissions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.ZeroEmissions;
                stopwatch.Stop();
                analytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    analytics.PrintUpdateSummary();
                }
                return false;
            }
            if (analytics.MaxTransitions.Count > 0 && analytics.MaxTransitions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                stopwatch.Stop();
                analytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    analytics.PrintUpdateSummary();
                }
                return false;
            }
            if (analytics.PropogatedTransitionValues.Count > 0 && analytics.PropogatedTransitionValues.Values.Sum() < double.Epsilon)
            {
                analytics.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                stopwatch.Stop();
                analytics.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    analytics.PrintUpdateSummary();
                }
                return false;
            }

            if (newProbabilityVector.GetSum() < double.Epsilon)
            {
                throw new Exception("New probability vector is zero everywhere. This problem should have been caught before this breakpoint is hit");
            }
            analytics.NonNormalizedProbabilityVector = newProbabilityVector;

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