using Snappy.Config;
using Snappy.Enums;
using Snappy.Functions;
using Snappy.MapMatching;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.OpenStreetMaps
{
    /// <summary>
    /// Class which helps to snap GPS tracks to OSM roads. 
    /// </summary>
    public class OsmSnapper
    {
        private string _overpassApi { get; set; }

        private bool _printConsoleUpdates { get; set; }

        public BoundingBox SnappingArea { get; set; }

        public OsmMapMatcher MapMatcher { get; set; }

        public MapMatcherParameters Parameters { get; set; }

        public SnapSummary SnapSummary { get; set; }

        /// <summary>
        /// Initialize OsmSnapper, which allows you to snap GPS tracks to on-road geometries
        /// </summary>
        /// <param name="overpassApi"></param>
        /// <param name="boundingBox"></param>
        /// <param name="parameters"></param>
        /// <param name="printConsoleUpdates"></param>
        public OsmSnapper(OverpassApi overpassApi, BoundingBox boundingBox = null, MapMatcherParameters parameters = null, bool printConsoleUpdates = false)
        {
            _printConsoleUpdates = printConsoleUpdates;
            if(parameters == null)
            {
                parameters = MapMatcherParameters.Default;
            }
            Parameters = parameters;

            if (overpassApi == OverpassApi.DeloreanGray) _overpassApi = Config.Urls.DeloreanGray;
            else if (overpassApi == OverpassApi.MainOverpass) _overpassApi = Config.Urls.MainOverpassApi;
            else throw new ArgumentException("Invalid overpass enum");

            if(boundingBox != null)
            {
                SnappingArea = boundingBox;
                // Build graph in bounding box and initialize map matcher (involves computing search grid data structure) 
                var graph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox);
                MapMatcher = new OsmMapMatcher(graph, parameters);
            }
        }

        /// <summary>
        /// Snaps track to OSM roads. Returns a list of snapped geometries - if it fails to find one continuous OSM geometry corresponding to the track, the list contain more than one geometry. 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="timeStamps"></param>
        /// <param name="highwayTags"></param>
        /// <param name="railTags"></param>
        /// <returns> List of snapped geometries. If it fails to find one continuous OSM geometry corresponding to the track, the list contains more than one geometry. Otherwise count = 1</returns>
        public List<List<Coord>> SnapDat(List<Coord> track, List<DateTime> timeStamps = null, bool highwayTags = true, bool railTags = true)
        {
            if (track.Count < 2) { return new List<List<Coord>>() { track }; }

            var result = new List<List<Coord>>();

            // Initialize total snap time stopwatch
            var totalTimeStopwatch = new System.Diagnostics.Stopwatch();
            totalTimeStopwatch.Start();


            OsmMapMatcher mapMatcher;//= new MapMatcher(osmGraph);
            if(MapMatcher == null)
            {
                // Build graph in region and initialize map matcher
                BoundingBox boundingBox = track.GetBoundingBox(DefaultValues.GPS_Error_In_Meters);
                var osmGraph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox, highwayTags, railTags);

                mapMatcher = new OsmMapMatcher(osmGraph, Parameters);
            }
            else
            {
                mapMatcher = MapMatcher.Clone();
            }

            // Initialize snap time stopwatch
            var performSnapStopwatch = new System.Diagnostics.Stopwatch();
            performSnapStopwatch.Start();

            // Clean input co-ordinates
            var cleanedCoords = track.GetCleanedCoordinates(timeStamps);

            // Initialize snap summary & list for update times
            SnapSummary = new SnapSummary();
            SnapSummary.UpdateCount = cleanedCoords.Count;
            var updateTimesInMilliseconds = new List<double>();

            int startIndex = 0;
            int breakIndex = 0;
            for (int i = 0; i < cleanedCoords.Count; i++)
            {
                Coord coord = cleanedCoords[i];
                if (mapMatcher.TryUpdateState(coord, printUpdateAnalyticsToConsole: _printConsoleUpdates))
                {
                    updateTimesInMilliseconds.Add(mapMatcher.UpdateInfo.UpdateTimeInMilliseconds);
                }
                else
                {
                    SnapSummary.BreakCount += 1;
                    breakIndex = i;
                    if(startIndex < breakIndex)
                    {
                        var shape = GetSnappedSection(mapMatcher, cleanedCoords, startIndex, breakIndex);
                        result.Add(shape);
                    }
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
            SnapSummary.PerformSnapTimeInSeconds = performSnapStopwatch.Elapsed.TotalSeconds;
            totalTimeStopwatch.Stop();
            SnapSummary.TotalSnapTimeInSeconds = totalTimeStopwatch.Elapsed.TotalSeconds;
            SnapSummary.MeanUpdateTimeInMilliseconds = updateTimesInMilliseconds.Average();

            // Print summary info to the console
            SnapSummary.PrintSummaryToConsole();

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
        private List<Coord> GetSnappedSection(OsmMapMatcher matcher, List<Coord> cleanShape, int startIndex, int breakIndex)
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

        public string GetSummary(string FilneName)
        {
            string theThing = "------------------ Snap Summary ------------------ \n" +
                    $"File Name:                {FilneName} \n" +               
                    $"Total snap time:          { TotalSnapTimeInSeconds} seconds \n" +
                    $"Time to perform snapping: {PerformSnapTimeInSeconds} seconds \n" +
                    $"Updates performed:        {UpdateCount} \n" +
                    $"Mean update time:         {MeanUpdateTimeInMilliseconds}ms \n" +
                    $"Map match breaks:         { BreakCount} \n";
            
            return theThing;
        }
        public string GetEntryCSVSummary(string FilneName, string MatchedStatus, List<Coord> gpxTrack)

        {
            var numberOfPoints = gpxTrack.Count;
            List<double> pointArray = new List<double>();

            for (int i = 1; i < gpxTrack.Count; i++)
            {
                var distance = DistanceFunctions.FasterHaversineDistance(gpxTrack[i - 1], gpxTrack[i]);
                pointArray.Add(distance);
            }

            double maxDistance = pointArray.Max();
            double minDistance = pointArray.Min();
            double meandistance = pointArray.Sum() / pointArray.Count;


            string theThing = $" {FilneName}, {TotalSnapTimeInSeconds}, {PerformSnapTimeInSeconds}, {UpdateCount}, {MeanUpdateTimeInMilliseconds}, {BreakCount}, {numberOfPoints}, {maxDistance}, {minDistance}, {meandistance}, {MatchedStatus} \n";

            return theThing;
        }


    }
}