using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Config;

namespace Snappy.MapMatching
{
    public class MapMatcherParameters
    {
        /// <summary>
        /// Parameter govorning transition probabilities. 
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// Parameter govorning emission probabilities. 
        /// </summary>
        public double Sigma { get; set; }

        /// <summary>
        /// Controls how close roads must be to an input co-ordinate to be considered candidates in the map matching algorithm. 
        /// </summary>
        public double NearbyRoadsThreshold { get; set; }
        
        /// <summary>
        /// Threshold difference value to not consider a transition as a possible transition (the diff being that between on-road distance and haversine distance) 
        /// </summary>
        public double TransitionDiffThreshold { get; set; }

        /// <summary>
        /// Upper limit on Dijstra pathfinding (if two nodes in the graph are not connected, or very far, the pathfinding algorithm is wasting its time) 
        /// </summary>
        public double DijstraUpperSearchLimit { get; set; }

        public MapMatcherParameters(double beta = DefaultValues.Beta_For_Transitions_In_Meters, double sigma = DefaultValues.Sigma_Value_In_Meters_For_Emissions, double nearbyRoadsThreshold = DefaultValues.Nearby_Road_Radius_In_Meters, double diffThreshold = DefaultValues.Difference_Threshold_For_Transitions_In_Meters, double dijstraUpperSearchLimit = DefaultValues.Dijstra_Upper_Search_Limit_In_Meters)
        {
            Beta = beta;
            Sigma = sigma;
            NearbyRoadsThreshold = nearbyRoadsThreshold;
            TransitionDiffThreshold = diffThreshold;
            DijstraUpperSearchLimit = dijstraUpperSearchLimit;
        }

        public static MapMatcherParameters Default
        {
            get
            {
                return new MapMatcherParameters();
            }
        }
    }
}
