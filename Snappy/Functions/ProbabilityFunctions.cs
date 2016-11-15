using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Functions
{
    public static class ProbabilityFunctions
    {
        public static double ExponentialDistribution(double input, double beta)
        {
            if (input < 0) { throw new ArgumentException("Must be a positive value"); }
            return (1 / beta) * Math.Exp(-input / beta);
        }

        //Normalized Half-Normal distribution
        public static double HalfGaussian(double distance, double sigma)
        {
            return (Math.Sqrt(2) / (sigma * Math.Sqrt(Math.PI))) * Math.Exp(-(distance * distance) / (2 * sigma * sigma));
        }
    }
}
