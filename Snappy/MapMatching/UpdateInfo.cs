using ConsoleTables.Core;
using Snappy.DataStructures;
using Snappy.Enums;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.MapMatching
{
    public class UpdateInfo
    {
        /// <summary>
        /// Probabilies before update
        /// </summary>
        public ProbabilityVector<DirectedRoad> PrevProbabilities { get; set; }

        /// <summary>
        /// Previous coordinate
        /// </summary>
        public Coord PrevCoordinate { get; set; }

        /// <summary>
        /// Probabilities after update ( & normalizing)
        /// </summary>
        public ProbabilityVector<DirectedRoad> Probabilities { get; set; }

        /// <summary>
        /// Co-ordinate at this update step
        /// </summary>
        public Coord Coordinate { get; set; }

        /// <summary>
        /// Stores all probability information for each road candidate at this step i.e. which road transitioned from, P(from), P(transition), P(emission)
        /// </summary>
        public Dictionary<DirectedRoad, CandidateInfo> CandidateDetails { get; set; }

        /// <summary>
        /// Stores the emission probabilities calculated at this step
        /// </summary>
        public Dictionary<DirectedRoad, Emission> Emissions { get; set; }

        /// <summary>
        /// Stores the values P(from)*P(transition), i.e. the propogated transition for the given road
        /// </summary>
        public Dictionary<DirectedRoad, double> Transitions { get; set; }

        /// <summary>
        ///  Stores all transition candidates at this step
        /// </summary>
        public Dictionary<DirectedRoad, List<Transition>> AllTransitions { get; set; }

        /// <summary>
        /// Probabilities after update, before normalizing
        /// </summary>
        public ProbabilityVector<DirectedRoad> NonNormalizedProbabilityVector { get; set; }

        /// <summary>
        /// Status of the update
        /// </summary>
        public MapMatchUpdateStatus UpdateStatus { get; set; }

        /// <summary>
        /// Time taken to perform update
        /// </summary>
        public double UpdateTimeInMilliseconds { get; set; }

        public UpdateInfo()
        {
            CandidateDetails = new Dictionary<DirectedRoad, CandidateInfo>();
            Emissions = new Dictionary<DirectedRoad, Emission>();
            Transitions = new Dictionary<DirectedRoad, double>();
            AllTransitions = new Dictionary<DirectedRoad, List<Transition>>();
        }

        public List<SummaryInfo> BuildInformationSummary()
        {
            var result = new List<SummaryInfo>();

            var roadsOrderedByProbability = Probabilities.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

            foreach (var road in roadsOrderedByProbability)
            {
                var info = new SummaryInfo
                {
                    StartEndNodeIds = string.Format("{0} -> {1}", road.Start, road.End),
                    EmissionProbability = Round(Emissions[road].Probability),
                    EmissionDistance = Round(Emissions[road].Distance.DistanceInMeters),
                    Probability = Probabilities[road],
                };

                // Assign road name
                if (road.Name != "")
                {
                    info.RoadName = road.Name;
                }
                else
                {
                    info.RoadName = "[NoName]";
                }

                if (PrevProbabilities.ContainsKey(road))
                {
                    info.PrevProbability = Round(PrevProbabilities[road]);
                }
                else
                {
                    info.PrevProbability = 0;
                }

                if (Transitions.Count > 0)
                {

                    CandidateInfo roadInfo = this.CandidateDetails[road];
                    // most probable transition to get to the current road
                    info.TransitionProbability = Round(roadInfo.P_Transition);

                    var fromRoad = roadInfo.From;

                    if (fromRoad.Name != "")
                    {
                        info.FromName = fromRoad.Name;
                    }
                    else
                    {
                        info.FromName = "[NoName]";
                    }
                    info.FromStartEndIds = string.Format("{0} -> {1}", fromRoad.Start, fromRoad.End);
                    info.PrevProbOfFrom = roadInfo.P_From;
                }
                result.Add(info);
            }
            return result;
        }

        private double Round(double input)
        {
            return Math.Round(input, 8);
        }

        public void PrintUpdateSummary(int count = -1)
        {
            Console.WriteLine("UPDATE:  {0}ms ", Round(this.UpdateTimeInMilliseconds));
            Console.WriteLine("AT:  {0}, {1} \n", this.Coordinate.Latitude, this.Coordinate.Longitude);

            switch (UpdateStatus)
            {
                case MapMatchUpdateStatus.SuccessfullyUpdated:
                    var summaryInfo = BuildInformationSummary();
                    if (count < 0) count = summaryInfo.Count;
                    if (summaryInfo.Count < count) count = summaryInfo.Count;

                    summaryInfo = summaryInfo.Take(count).ToList();

                    while (count > 6)
                    {
                        var table = MakeTable(summaryInfo.Take(6).ToList());
                        table.Write(Format.MarkDown);
                        summaryInfo = summaryInfo.Skip(6).ToList();
                        count = summaryInfo.Count;
                    }

                    var lastTable = MakeTable(summaryInfo);
                    lastTable.Write(Format.MarkDown);
                    break;

                case MapMatchUpdateStatus.NoNearbyRoads:
                    Console.WriteLine("FAILED: No nearby roads \n");
                    break;

                case MapMatchUpdateStatus.NoPossibleTransitions:
                    Console.WriteLine("FAILED: No nearby roads \n");
                    break;

                case MapMatchUpdateStatus.ZeroEmissions:
                    Console.WriteLine("FAILED: Zero emissions \n");
                    break;
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