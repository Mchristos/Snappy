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

        public void UpdateState(Coord coord, DateTime timeStamp = default(DateTime))
        {
            List<DirectedRoad> nearbyRoads = SearchGrid.GetNearbyValues(coord, Constants.Search_Grid_Grid_Size_In_Meters);
            List<ProjectToRoad> nearbyRoadProjections = nearbyRoads.Select(x => new ProjectToRoad(coord, x)).ToList();

            // Initialize new transition memory 
            var transitionMemory = new Dictionary<DirectedRoad, DirectedRoad>();
            // Initialize new probability vector 
            var newProbabilityVector = new ProbabilityVector<DirectedRoad>();

            if(State.PrevCoord == null)
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    double emission = MarkovProbabilityHelpers.EmissionProbability(projection);
                    newProbabilityVector[projection.Road] = emission;
                    transitionMemory[projection.Road] = null;
                }
            }
            else
            {
                foreach (var projection in nearbyRoadProjections)
                {
                    //Calculate emission probability
                    double emission = MarkovProbabilityHelpers.EmissionProbability(projection);

                    //Calculate maximum transition from possible prev state 
                    var maxCandidates = new Dictionary<DirectedRoad, double>();
                    foreach (var prevProjection in State.PrevNearbyRoadsAndProjections)
                    {
                        double transition = MarkovProbabilityHelpers.TransitionProbability(Graph, prevProjection, projection);
                        maxCandidates[prevProjection.Road] = transition * State.Probabilities[prevProjection.Road];
                    }
                    var maxPair = maxCandidates.Aggregate((x, y) => x.Value > y.Value ? x : y);

                    //probability update
                    newProbabilityVector[projection.Road] = maxPair.Value * emission;

                    //transition memory 
                    transitionMemory[projection.Road] = maxPair.Key;
                }
            }


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
        }
        public void Reset()
        {
            State.Reset();
        }
    }
}
