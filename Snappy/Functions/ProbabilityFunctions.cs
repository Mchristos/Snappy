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

        public static double Gaussian(double distance, double sigma)
        {
            return (1 / sigma * Config.Constants.Square_Root_Of_Two_Pi) * Math.Exp(-(distance * distance) / (2 * sigma * sigma));
        }
    }
}
