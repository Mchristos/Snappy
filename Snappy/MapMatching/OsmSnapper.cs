using Snappy.Config;
using Snappy.Enums;
using Snappy.Functions;
using Snappy.OpenStreetMaps;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.MapMatching
{
    public class OsmSnapper
    {
        private string _overpassApi { get; set; }

        private bool _printConsoleUpdates { get; set; }

        public BoundingBox SnappingArea { get; set; }

        public MapMatcher MapMatcher { get; set; }

        public OsmSnapper(OverpassApi overpassApi, BoundingBox boundingBox = null,  bool printConsoleUpdates = false)
        {
            _printConsoleUpdates = printConsoleUpdates;

            if (overpassApi == OverpassApi.DeloreanGray) _overpassApi = Config.Urls.DeloreanGray;
            else if (overpassApi == OverpassApi.MainOverpass) _overpassApi = Config.Urls.MainOverpassApi;
            else throw new ArgumentException("Invalid overpass enum");

            if(boundingBox != null)
            {
                SnappingArea = boundingBox;
                // Build graph in bounding box and initialize map matcher (involves computing search grid data structure) 
                var graph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox);
                MapMatcher = new MapMatcher(graph);
            }
        }

        public List<List<Coord>> SnapDat(List<Coord> coords, List<DateTime> timeStamps = null, bool highwayTags = true, bool railTags = true)
        {
            if (coords.Count < 2) { throw new ArgumentException("Sequence has less than two co-ordinates."); }

            var result = new List<List<Coord>>();

            // Initialize total snap time stopwatch
            var totalTimeStopwatch = new System.Diagnostics.Stopwatch();
            totalTimeStopwatch.Start();


            MapMatcher mapMatcher;//= new MapMatcher(osmGraph);
            if(MapMatcher == null)
            {
                // Build graph in region and initialize map matcher
                BoundingBox boundingBox = coords.GetBoundingBox(Constants.GPS_Error_In_Meters);
                var osmGraph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox, highwayTags, railTags);

                mapMatcher = new MapMatcher(osmGraph);
            }
            else
            {
                mapMatcher = MapMatcher.Clone();
            }

            // Initialize snap time stopwatch
            var performSnapStopwatch = new System.Diagnostics.Stopwatch();
            performSnapStopwatch.Start();

            // Clean input co-ordinates
            var cleanedCoords = coords.GetCleanedCoordinates(timeStamps);

            // Initialize snap summary & list for update times
            var snapSummary = new SnapSummary();
            snapSummary.UpdateCount = cleanedCoords.Count;
            var updateTimesInMilliseconds = new List<double>();

            int startIndex = 0;
            int breakIndex = 0;
            for (int i = 0; i < cleanedCoords.Count; i++)
            {
                Coord coord = cleanedCoords[i];
                UpdateAnalytics analytics;
                if (mapMatcher.TryUpdateState(coord, out analytics, printUpdateAnalyticsToConsole: _printConsoleUpdates))
                {
                    updateTimesInMilliseconds.Add(analytics.UpdateTimeInMilliseconds);
                }
                else
                {
                    snapSummary.BreakCount += 1;
                    breakIndex = i;
                    var shape = GetSnappedSection(mapMatcher, cleanedCoords, startIndex, breakIndex);
                    result.Add(shape);
                    startIndex = i + 1;
                    mapMatcher.Reset();
                }
            }

            if (startIndex < cleanedCoords.Count - 1)
            {
                var lastShape = GetSnappedSection(mapMatcher, cleanedCoords, startIndex, cleanedCoords.Count - 1);
                result.Add(lastShape);
            }

            // Snap summary values
            performSnapStopwatch.Stop();
            snapSummary.PerformSnapTimeInSeconds = performSnapStopwatch.Elapsed.TotalSeconds;
            totalTimeStopwatch.Stop();
            snapSummary.TotalSnapTimeInSeconds = totalTimeStopwatch.Elapsed.TotalSeconds;
            snapSummary.MeanUpdateTimeInMilliseconds = updateTimesInMilliseconds.Average();

            // Print summary info to the console
            snapSummary.PrintSummaryToConsole();

            return result;
        }

        /// <summary>
        /// Gets the snapped geometry from the map matcher's current state (potentially after the map matcher breaks)
        /// </summary>
        /// <param name="matcher"></param>
        /// <param name="cleanShape"></param>
        /// <param name="startIndex"></param>
        /// <param name="breakIndex"></param>
        /// <returns></returns>
        private List<Coord> GetSnappedSection(MapMatcher matcher, List<Coord> cleanShape, int startIndex, int breakIndex)
        {
            var sequenceSoFar = matcher.State.GetMostLikelySequence();
            var connectedSequence = PathFinding.DijstraConnectUpSequence(sequenceSoFar, matcher.Graph);
            return TrimRoadSequence(connectedSequence.Select(st => st.Geometry).ToList(), cleanShape[startIndex], cleanShape[breakIndex]);
        }

        /// <summary>
        /// Trims the first and last roads in a sequence of road geometries w.r.t. projections of starting / ending co-ordinates.
        /// </summary>
        /// <param name="roads"></param>
        /// <param name="startingCoord"></param>
        /// <param name="endingCoord"></param>
        /// <returns></returns>
        private List<Coord> TrimRoadSequence(List<List<Coord>> roads, Coord startingCoord, Coord endingCoord)
        {
            if (roads.Count == 1)
            {
                return roads.First().TrimGeometryBetween(startingCoord, endingCoord);
            }

            List<Coord> result = new List<Coord>();
            // add starting shape
            var startingShape = roads.First().TrimGeometryFrom(startingCoord);
            result.AddRange(startingShape);
            // add middle part
            for (int i = 1; i < roads.Count - 1; i++)
            {
                result.AddRange(roads[i].Skip(1));
            }
            //add ending shape
            var endingShape = roads.Last().TrimGeometryUntil(endingCoord);
            result.AddRange(endingShape.Skip(1));
            return result;
        }
    }

    public class SnapSummary
    {
        public double TotalSnapTimeInSeconds { get; set; }

        public double PerformSnapTimeInSeconds { get; set; }

        public double MeanUpdateTimeInMilliseconds { get; set; }

        public int UpdateCount { get; set; }
        public int BreakCount { get; set; }

        public SnapSummary()
        {
            BreakCount = 0;
        }

        public void PrintSummaryToConsole()
        {
            Console.WriteLine("");
            Console.WriteLine("------------------ Snap Summary ------------------");
            Console.WriteLine("Total snap time:          {0} seconds \n", TotalSnapTimeInSeconds);
            Console.WriteLine("Time to perform snapping: {0} seconds", PerformSnapTimeInSeconds);
            Console.WriteLine("Updates performed:        {0}", UpdateCount);
            Console.WriteLine("Mean update time:         {0}ms", MeanUpdateTimeInMilliseconds);
            Console.WriteLine("Map match breaks:         {0}", BreakCount);
            Console.WriteLine("--------------------------------------------------");
        }
    }
}