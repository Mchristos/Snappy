using Snappy.Config;
using Snappy.DataStructures;
using Snappy.Functions;
using System;
using System.Diagnostics;

namespace Snappy.MapMatching
{
    // Contains all the information of a possible transition, inclusing the transition probability
    [DebuggerDisplay("{Probability}")]
    public class Transition
    {
        public double Probability { get; set; }

        public double OnRoadDistInMeters { get; set; }

        public double HaversineDistInMeters { get; set; }

        public DirectedRoad From { get; set; }

        public DirectedRoad To { get; set; }

        public Transition(double onRoadDistanceInMeters, double haversineDistanceInMeters, DirectedRoad from, DirectedRoad to, MapMatcherParameters parameters)
        {
            OnRoadDistInMeters = onRoadDistanceInMeters;
            HaversineDistInMeters = haversineDistanceInMeters;
            From = from;
            To = to;
            
            // COMPUTE TRANSITION PROBABILITY
            double diffInMeters = Math.Abs(haversineDistanceInMeters - onRoadDistanceInMeters);
            if(diffInMeters > parameters.TransitionDiffThreshold)
            {
                Probability = 0;
            }
            else
            {
                Probability = ProbabilityFunctions.ExponentialDistribution(diffInMeters, parameters.Beta);
            }
        }

        private Transition() { }
        public static Transition ImpossibleTransition(DirectedRoad from, DirectedRoad to)
        {
            var result = new Transition();
            result.From = from;
            result.To = to;
            result.Probability = 0;
            result.OnRoadDistInMeters = double.NaN;
            result.HaversineDistInMeters = double.NaN;
            return result;
        }

    }
}