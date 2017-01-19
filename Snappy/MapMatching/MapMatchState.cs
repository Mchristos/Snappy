using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Functions;

namespace Snappy.MapMatching
{
    public class MapMatchState
    {
        public ProbabilityVector<DirectedRoad> Probabilities { get; set; }
        public Coord PrevCoord { get; set; }
        public DateTime PrevTime { get; set; }
        public List<Dictionary<DirectedRoad, DirectedRoad>> TransitionMemory { get; set; }
        public List<ProjectToRoad> PrevNearbyRoadsAndProjections { get; set; }

        private MapMatchState()
        {

        }

        public static MapMatchState InitialState()
        {
            var result = new MapMatchState();
            result.Probabilities = new ProbabilityVector<DirectedRoad>();
            result.TransitionMemory = new List<Dictionary<DirectedRoad, DirectedRoad>>();
            return result;
        }

        public void Reset()
        {
            this.Probabilities = new ProbabilityVector<DirectedRoad>();
            this.PrevCoord = null;
            this.PrevTime = default(DateTime);
            this.TransitionMemory = new List<Dictionary<DirectedRoad, DirectedRoad>>();
            this.PrevNearbyRoadsAndProjections = null;
        }


        public List<DirectedRoad> GetMostLikelySequence()
        {
            if(Probabilities != null && Probabilities.Count > 0)
            {
                // get most probable road
                DirectedRoad mostLikelyRoad = Probabilities.GetMostProbableItem();
                // trace back most likely sequence
                List<DirectedRoad> result = TransitionMemory.TraceBackSequence(mostLikelyRoad);
                return result;
            }
            else
            {
                return new List<DirectedRoad>();
            }
        }
    }
}
