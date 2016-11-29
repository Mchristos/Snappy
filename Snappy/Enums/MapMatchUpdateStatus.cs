using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Enums
{
    public enum MapMatchUpdateStatus
    {
        //Invalid value
        Default,

        // Successfully updated
        SuccessfullyUpdated,

        // Map matcher breaks: 

        // No nearby roads we found at this step
        NoNearbyRoads,

        // All emission probabilities are zero (this should never happen basically, because there are nearby roads) 
        ZeroEmissions,

        // All possible transitions from previous roads are impossible 
        NoPossibleTransitions
    }
}
