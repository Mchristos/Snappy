using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.MapMatching
{
    public class CandidateInfo
    {
        public CandidateInfo(DirectedRoad from, double p_from, double p_transition, Emission emission, double p_final)
        {
            From = from;
            P_From = p_from;
            P_Transition = p_transition;
            Emission = emission;
            P_Final = p_final;
        }
        public CandidateInfo()
        {

        }
        public DirectedRoad From { get; set; }

        public double P_From { get; set; }

        public double P_Transition { get; set; }

        public Emission Emission { get; set; }

        public double P_Final { get; set; }
    }
}
