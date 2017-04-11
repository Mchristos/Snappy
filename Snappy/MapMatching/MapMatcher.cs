using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Snappy.Functions;

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

        public UpdateInfo UpdateInfo { get; set; }

        private Dictionary<string, T> _dataByRoadId { get; set; }

        public MapMatcher(List<T> data, Func<T, DirectedRoad> dataToRoad, MapMatcherParameters parameters, BoundingBox boundingBox = null, bool useSearchGrid = true)
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
            if (useSearchGrid)
            {
                SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, parameters.NearbyRoadsThreshold, boundingBox);
            }

            // Initialize state
            State = MapMatchState.InitialState();
        }

        public MapMatcher()
        {
        }

        public bool TryUpdateState(Coord coord, DateTime timeStamp = default(DateTime), bool printUpdateAnalyticsToConsole = false)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            UpdateInfo = new UpdateInfo();
            UpdateInfo.PrevCoordinate = State.PrevCoord;
            UpdateInfo.Coordinate = coord;
            UpdateInfo.PrevProbabilities = State.Probabilities;

            // Find nearby roads using search grid
            List<DirectedRoad> nearbyRoads = GetNearbyRoads(coord, Parameters.NearbyRoadsThreshold);
            if (nearbyRoads.Count == 0)
            {
                // If no nearby roads, update fails.
                UpdateInfo.UpdateStatus = Enums.MapMatchUpdateStatus.NoNearbyRoads;
                stopwatch.Stop();
                UpdateInfo.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    UpdateInfo.PrintUpdateSummary();
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
                    // Get emission
                    Emission emission = MarkovProbabilityHelpers.EmissionProbability(projection, Parameters.Sigma);

                    // Log info
                    var candidateInfo = new CandidateInfo();
                    candidateInfo.Emission = emission;
                    UpdateInfo.CandidateDetails[projection.Road] = candidateInfo;
                    UpdateInfo.Emissions[projection.Road] = emission;

                    // Update probability and transition memory
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
                    UpdateInfo.Emissions[projection.Road] = emission;

                    //Calculate maximum transition from possible prev state
                    var maxCandidates = new Dictionary<Transition, double>();
                    UpdateInfo.AllTransitions[projection.Road] = new List<Transition>();
                    foreach (var prevProjection in State.PrevNearbyRoadsAndProjections)
                    {
                        Transition transition = MarkovProbabilityHelpers.TransitionProbability(Graph, prevProjection, projection, Parameters);
                        UpdateInfo.AllTransitions[projection.Road].Add(transition);
                        maxCandidates[transition] = transition.Probability * State.Probabilities[prevProjection.Road];
                    }
                    var maxPair = maxCandidates.Aggregate((x, y) => x.Value > y.Value ? x : y);

                    // Log info
                    UpdateInfo.Transitions[projection.Road] = maxPair.Value;
                    var candidateInfo = new CandidateInfo();
                    candidateInfo.Emission = emission;
                    candidateInfo.From = maxPair.Key.From;
                    candidateInfo.P_From = State.Probabilities[maxPair.Key.From];
                    candidateInfo.P_Transition = maxPair.Key.Probability;
                    candidateInfo.P_Final = maxPair.Value * emission.Probability;
                    UpdateInfo.CandidateDetails[projection.Road] = candidateInfo;

                    // Update probability and transition memory
                    newProbabilityVector[projection.Road] = maxPair.Value * emission.Probability;
                    transitionMemory[projection.Road] = maxPair.Key.From;
                }
            }

            UpdateInfo.NonNormalizedProbabilityVector = newProbabilityVector;
            UpdateInfo.Probabilities = newProbabilityVector.Normalize();

            // CHECK IF UPDATE FAILED:
            // Emission probabilities were practically zero (i.e. no nearby roads)
            if (UpdateInfo.Emissions.Values.Select(x => x.Probability).Sum() < double.Epsilon)
            {
                UpdateInfo.UpdateStatus = Enums.MapMatchUpdateStatus.ZeroEmissions;
                stopwatch.Stop();
                UpdateInfo.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    UpdateInfo.PrintUpdateSummary();
                }
                return false;
            }
            // No transitions were possible
            if (UpdateInfo.Transitions.Count > 0 && UpdateInfo.Transitions.Values.Sum() < double.Epsilon)
            {
                UpdateInfo.UpdateStatus = Enums.MapMatchUpdateStatus.NoPossibleTransitions;
                stopwatch.Stop();
                UpdateInfo.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    UpdateInfo.PrintUpdateSummary();
                }
                return false;
            }
            if (newProbabilityVector.GetSum() < double.Epsilon)
            {
                UpdateInfo.UpdateStatus = Enums.MapMatchUpdateStatus.CrossoverProblem;
                stopwatch.Stop();
                UpdateInfo.UpdateTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                if (printUpdateAnalyticsToConsole)
                {
                    UpdateInfo.PrintUpdateSummary();
                }
                return false;
            }

            //UPDATE STATE:
            newProbabilityVector = newProbabilityVector.Normalize();

            // 1. Update probability vector
            State.Probabilities = newProbabilityVector;

            //2. Add to transition memory
            State.TransitionMemory.Add(transitionMemory);

            //3. Update prev coord and date
            State.PrevCoord = coord;
            State.PrevTime = timeStamp;

            // 4.
            State.PrevNearbyRoadsAndProjections = nearbyRoadProjections;

            //LastUpdateAnalytics.ProbabilityVector = State.Probabilities;
            UpdateInfo.UpdateStatus = Enums.MapMatchUpdateStatus.SuccessfullyUpdated;
            stopwatch.Stop();
            UpdateInfo.UpdateTimeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            //Console.WriteLine(analytics.BuildSummaryString(5));

            if (printUpdateAnalyticsToConsole)
            {
                UpdateInfo.PrintUpdateSummary(20);
            }
            return true;
        }

        private List<DirectedRoad> GetNearbyRoads(Coord query, double radiusInMeters)
        {
            if(SearchGrid != null)
            {
                return SearchGrid.GetNearbyValues(query, radiusInMeters);
            }
            else
            {
                int i;
                return Graph.Roads.Where(x => query.SnapToPolyline(x.Geometry, out i).HaversineDistance(query).DistanceInMeters < radiusInMeters).ToList();
            }
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