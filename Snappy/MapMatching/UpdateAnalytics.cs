using Snappy.DataStructures;
using Snappy.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.MapMatching
{
    public class UpdateAnalytics
    {
        // Stores the emission probabilities at this step
        public Dictionary<DirectedRoad, Emission> Emissions { get; set; }

        public Dictionary<DirectedRoad, Transition> MaxTransitions { get; set; }

        public List<Transition> AllTransitions { get; set; }

        public ProbabilityVector<DirectedRoad> PrevProbabilityVector { get; set; }
        public ProbabilityVector<DirectedRoad> NewProbabilityVector { get; set; }

        public MapMatchUpdateStatus UpdateStatus { get; set; }

        public UpdateAnalytics()
        {
            Emissions = new Dictionary<DirectedRoad, Emission>();
            MaxTransitions = new Dictionary<DirectedRoad, Transition>();

            AllTransitions = new List<Transition>();
        }

        public List<SummaryInfo> BuildInformationSummary()
        {
            var result = new List<SummaryInfo>();

            var roadsOrderedByProbability = NewProbabilityVector.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

            foreach (var road in roadsOrderedByProbability)
            {
                var info = new SummaryInfo
                {
                    RoadName = string.Format("{0} ({1} -> {2})", road.Name, road.Start % 1000, road.End % 1000),
                    EmissionProbability = Emissions[road].Probability,
                    EmissionDistance = Emissions[road].Distance.DistanceInMeters,
                    Probability = NewProbabilityVector[road]
                };
                if (PrevProbabilityVector.ContainsKey(road))
                {
                    info.PrevProbability = PrevProbabilityVector[road];
                }
                else
                {
                    info.PrevProbability = 0;
                }

                if(MaxTransitions.Count > 0)
                {
                    // most probable transition to get to the current road
                    var transition = MaxTransitions[road];

                    var fromRoad = transition.From;
                    info.FromName = string.Format("{0} ({1} -> {2})", fromRoad.Name, fromRoad.Start % 1000, fromRoad.End % 1000);
                    info.TransitionProbability = transition.Probability;

                    if (PrevProbabilityVector.ContainsKey(fromRoad))
                    {
                        info.PrevProbOfFrom = PrevProbabilityVector[fromRoad];
                    }
                    else
                    {
                        info.PrevProbOfFrom = 0;
                    }
                }

                result.Add(info);
            }
            return result;
        }


        public string BuildSummaryString(int count = -1)
        {
            string result = "UPDATE \n";

            var summaryInfo = BuildInformationSummary();
            if (count < 0) count = summaryInfo.Count;
            if (summaryInfo.Count < count) count = summaryInfo.Count;

            for (int i = 0; i < count; i++)
            {
                var info = summaryInfo[i];

                result += "--------------------------------------------- \n";
                result += string.Format("{0} \n", info.RoadName);
                result += string.Format("{0} \n", info.Probability);
                result += "\n \n";
                if (info.HasTransitionInfo)
                {
                    result += string.Format("From \t \t {0} \n", info.FromName);
                    result += string.Format("Transition \t {0} \n", info.TransitionProbability);
                    result += string.Format("P(From) \t {0} \n", info.PrevProbOfFrom);
                }
                result += string.Format("Emission \t {0} (dist {1} m) \n", info.EmissionProbability, info.EmissionDistance);
                result += "---------------------------------------------";
                result += "\n \n";
            }
            result += "\n\n\n";
            return result;
        }



    }

    public class SummaryInfo
    {
        //Indicates if there is transition info in this object
        public bool HasTransitionInfo
        {
            get { return FromName != null; }
        }

        // Road in question 
        public string RoadName { get; set; }

        // Probability of road in question after update (at "current" time step) 
        public double Probability { get; set; }

        // Name of road it transitioned from
        public string FromName { get; set; }

        // Transition probability
        public double TransitionProbability { get; set; }

        // Probability of road transitioned from at the last step
        public double PrevProbOfFrom { get; set; }

        // Emission Probability
        public double EmissionProbability { get; set; }

        //Emission distance 
        public double EmissionDistance { get; set; }

        // Probability of road in question before update 
        public double PrevProbability { get; set; }
    }
}