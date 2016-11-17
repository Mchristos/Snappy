using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Enums;

namespace Snappy.MapMatching
{
    public class UpdateAnalytics
    {
        // Stores the emission probabilities at this step 
        public ProbabilityVector<DirectedRoad> EmissionProbabilities { get; set; }

        public ProbabilityVector<Tuple<DirectedRoad, DirectedRoad>> AllTransitionProbabilities { get; set; }

        public ProbabilityVector<DirectedRoad> PrevProbabilityVector { get; set; }

        public ProbabilityVector<Tuple<DirectedRoad, DirectedRoad>> MostProbableTransitions { get; set; }


        public MapMatchUpdateStatus UpdateStatus { get; set; }

        public UpdateAnalytics()
        {
            EmissionProbabilities = new ProbabilityVector<DirectedRoad>();
            AllTransitionProbabilities = new ProbabilityVector<Tuple<DirectedRoad, DirectedRoad>>();
            MostProbableTransitions = new ProbabilityVector<Tuple<DirectedRoad, DirectedRoad>>();
        }










    }
}
