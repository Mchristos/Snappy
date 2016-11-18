using Snappy.Config;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.MapMatching
{
    // Contains the informations for an emission probability 
    // (probability of a vehicle on a road "emitting" a GPS co-ordinate)
    public class Emission
    {

        public double Probability { get; set; }

        public Distance Distance { get; set; }


        public Emission(ProjectToRoad projection)
        {
            Probability = ProbabilityFunctions.HalfGaussian(projection.ProjectedDistance.DistanceInMeters, Constants.Sigma_Value_In_Meters_For_Emissions);
            Distance = projection.ProjectedDistance;
        }

    }
}
