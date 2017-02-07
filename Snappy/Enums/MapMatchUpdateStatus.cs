using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Enums
{
    public enum MapMatchUpdateStatus
    {
        /// <summary>
        /// Invalid value
        /// </summary>
        Default,

        /// <summary>
        /// Successfully updated
        /// </summary>
        SuccessfullyUpdated,

        // Map matcher breaks: 

        /// <summary>
        /// No nearby roads we found at this step
        /// </summary>
        NoNearbyRoads,

        /// <summary>
        /// All emission probabilities are zero (should be rare because there will be no nearby roads) 
        /// </summary>
        ZeroEmissions,
        
        /// <summary>
        ///  No candidate transitions are possible  
        /// </summary>
        NoPossibleTransitions,

        /// <summary>
        /// Only possible candidate transitions go to roads with zero emission 
        /// </summary>
        CrossoverProblem


    }
}
