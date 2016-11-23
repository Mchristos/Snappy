using Snappy.DataStructures;
using Snappy.Enums;
using System.Collections.Generic;
using System.Linq;
using System;
using Snappy.ValueObjects;
using ConsoleTables.Core;

namespace Snappy.MapMatching
{
    public class UpdateAnalytics
    {
        public Coord Coordinate { get; set; }
        // Stores the emission probabilities calculated at this step
        public Dictionary<DirectedRoad, Emission> Emissions { get; set; }
        // Stores the most likely transition to each nearby road at this step
        public Dictionary<DirectedRoad, Transition> MaxTransitions { get; set; }
        // Stores all transition candidates at this step 
        public Dictionary<DirectedRoad, List<Transition>> AllTransitions { get; set; }
        // Stores the probability vector before update 
        public ProbabilityVector<DirectedRoad> PrevProbabilityVector { get; set; }

        //Stores the probability vector after update (before normalizing) 
        public ProbabilityVector<DirectedRoad> NonNormalizedProbabilityVector { get; set; }

        //Stores the probability vector after update (after normalizing) 
        public ProbabilityVector<DirectedRoad> ProbabilityVector { get; set; }

        // Status of the update 
        public MapMatchUpdateStatus UpdateStatus { get; set; }
        
        // Time taken to perform update 
        public double UpdateTimeInMilliseconds { get; set; }

        public UpdateAnalytics()
        {
            Emissions = new Dictionary<DirectedRoad, Emission>();
            MaxTransitions = new Dictionary<DirectedRoad, Transition>();
            AllTransitions = new Dictionary<DirectedRoad, List<Transition>>();
        }

        public List<SummaryInfo> BuildInformationSummary()
        {
            var result = new List<SummaryInfo>();

            var roadsOrderedByProbability = ProbabilityVector.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

            foreach (var road in roadsOrderedByProbability)
            {
                var info = new SummaryInfo
                {
                    StartEndNodeIds = string.Format("{0} -> {1}", road.Start % 1000, road.End % 1000),
                    EmissionProbability = Round(Emissions[road].Probability),
                    EmissionDistance = Round(Emissions[road].Distance.DistanceInMeters),
                    Probability = Round(ProbabilityVector[road]),
                    NonNormalizedProbability = Round(NonNormalizedProbabilityVector[road])
                };

                // Assign road name 
                if(road.Name != "")
                {
                    info.RoadName = road.Name;
                }
                else
                {
                    info.RoadName = "[NoName]";
                }


                
                if (PrevProbabilityVector.ContainsKey(road))
                {
                    info.PrevProbability = Round(PrevProbabilityVector[road]);
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

                    if (fromRoad.Name != "")
                    {
                        info.FromName = fromRoad.Name;
                    }
                    else
                    {
                        info.FromName = "[NoName]";
                    }
                    info.FromStartEndIds = string.Format("{0} -> {1}", fromRoad.Start % 1000, fromRoad.End % 1000);
                    info.TransitionProbability = Round(transition.Probability);

                    if (PrevProbabilityVector.ContainsKey(fromRoad))
                    {
                        info.PrevProbOfFrom = Round(PrevProbabilityVector[fromRoad]);
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


        private double Round(double input)
        {
            return Math.Round(input, 8);
        }

        //public string BuildSummaryString(int count = -1)
        //{
        //    string result = string.Format("UPDATE:  {0}ms \n", Math.Round( this.UpdateTimeInMilliseconds, 4));

        //    var summaryInfo = BuildInformationSummary();
        //    if (count < 0) count = summaryInfo.Count;
        //    if (summaryInfo.Count < count) count = summaryInfo.Count;

        //    for (int i = 0; i < count; i++)
        //    {
        //        var info = summaryInfo[i];

        //        result += "--------------------------------------------- \n";
        //        result += string.Format("{0} \n", info.RoadName);
        //        result += string.Format("{0} \n", info.Probability);
        //        result += "\n \n";
        //        if (info.HasTransitionInfo)
        //        {
        //            result += string.Format("From \t \t {0} \n", info.FromName);
        //            result += string.Format("Transition \t {0} \n", info.TransitionProbability);
        //            result += string.Format("P(From) \t {0} \n", info.PrevProbOfFrom);
        //        }
        //        result += string.Format("Emission \t {0} (dist {1} m) \n", info.EmissionProbability, info.EmissionDistance);
        //        result += "---------------------------------------------";
        //        result += "\n \n";
        //    }
        //    result += "\n\n\n";
        //    return result;
        //}
        public void PrintUpdateSummary(int count = -1)
        {
            var summaryInfo = BuildInformationSummary();
            if (count < 0) count = summaryInfo.Count;
            if (summaryInfo.Count < count) count = summaryInfo.Count;

            summaryInfo = summaryInfo.Take(count).ToList();


            Console.WriteLine("UPDATE:  {0}ms ", Round(this.UpdateTimeInMilliseconds));
            Console.WriteLine("AT:  {0}, {1} \n", this.Coordinate.Latitude, this.Coordinate.Longitude);
            if (count > 6)
            {
                var table1 = MakeTable(summaryInfo.Take(6).ToList());
                var table2 = MakeTable(summaryInfo.Skip(6).ToList());
                table1.Write(Format.MarkDown);
                table2.Write(Format.MarkDown);    
            }
            else
            {
                var table = MakeTable(summaryInfo);
                table.Write(ConsoleTables.Core.Format.MarkDown);

            }




        }

        private ConsoleTables.Core.ConsoleTable MakeTable(List<SummaryInfo> tableData)
        {
            var columnHeading = MakeRow("", tableData.Select(x => x.RoadName).ToList());
            ConsoleTables.Core.ConsoleTable result = new ConsoleTables.Core.ConsoleTable(columnHeading);


            var startEndNodes = MakeRow("", tableData.Select(x => x.StartEndNodeIds));
            var probabilities = MakeRow("Prob", tableData.Select(x => x.Probability.ToString()));
            var emptyRow = MakeRow("", tableData.Select(x => ""));
            var emissions = MakeRow("Emmission", tableData.Select(x => string.Format("{0} ({1} m)", x.EmissionProbability, x.EmissionDistance)));
            var transitions = MakeRow("Transition", tableData.Select(x => x.TransitionProbability.ToString()));
            var pOfFrom = MakeRow("P(From)", tableData.Select(x => x.PrevProbOfFrom.ToString()));
            var fromNames = MakeRow("From", tableData.Select(x => x.FromName));
            var fromStartEndIds = MakeRow("", tableData.Select(x => x.FromStartEndIds));

            result.AddRow(startEndNodes);
            result.AddRow(probabilities);
            result.AddRow(emptyRow);
            result.AddRow(emissions);
            result.AddRow(transitions);
            result.AddRow(pOfFrom);
            result.AddRow(fromNames);
            result.AddRow(fromStartEndIds);

            return result;
        }

        private string[] MakeRow(string rowTitle, IEnumerable<string> values)
        {
            var result = new List<string>() { rowTitle };
            foreach (var value in values)
            {
                result.Add(value);
            }
            return result.ToArray();
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

        // Start / end node Ids 
        public string StartEndNodeIds { get; set; }

        // Probability of road in question after update (at "current" time step) 
        public double Probability { get; set; }

        public double NonNormalizedProbability { get; set; }

        // Name of road it transitioned from
        public string FromName { get; set; }

        public string FromStartEndIds { get; set; }

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