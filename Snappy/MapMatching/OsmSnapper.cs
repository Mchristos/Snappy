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

        public OsmSnapper(OverpassApi overpassApi)
        {
            if (overpassApi == OverpassApi.LocalDelorean) _overpassApi = Config.Urls.DeloreanGray;
            else if (overpassApi == OverpassApi.MainOverpass) _overpassApi = Config.Urls.MainOverpassApi;
            else throw new ArgumentException("Invalid overpass enum");
        }

        public List<List<Coord>> SnapDat(List<Coord> coords)
        {
            var result = new List<List<Coord>>();

            // Initialize total snap time stopwatch
            var totalTimeStopwatch = new System.Diagnostics.Stopwatch();
            totalTimeStopwatch.Start();

            // Build graph in region and initialize map matcher
            BoundingBox boundingBox = coords.GetBoundingBox();
            var osmGraph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox);
            var mapMatcher = new MapMatcher(osmGraph);

            // Initialize snap time stopwatch
            var performSnapStopwatch = new System.Diagnostics.Stopwatch();
            performSnapStopwatch.Start();

            // Clean input co-ordinates
            var cleanedCoords = coords.GetCleanedCoordinates();

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
                if (mapMatcher.TryUpdateState(coord, out analytics))
                {
                    updateTimesInMilliseconds.Add(analytics.UpdateTimeInMilliseconds);
                }
                else
                {
                    snapSummary.BreakCount += 1;
                    breakIndex = i;
                    var shape = GetSnappedSection(mapMatcher, cleanedCoords, startIndex, breakIndex);
                    result.Add(shape);
                    startIndex = i+ 1;
                    mapMatcher.Reset();
                }
            }

            if(startIndex < cleanedCoords.Count - 1)
            {
                var lastShape = GetSnappedSection(mapMatcher, cleanedCoords, startIndex, cleanedCoords.Count-1);
                result.Add(lastShape);
            }
            
            // Snap summary values
            performSnapStopwatch.Stop();
            snapSummary.PerformSnapTimeInSeconds = performSnapStopwatch.Elapsed.TotalSeconds; 
            totalTimeStopwatch.Stop();
            snapSummary.TotalSnapTimeInSeconds = totalTimeStopwatch.Elapsed.TotalSeconds;
            snapSummary.MeanUpdateTimeInMilliseconds = updateTimesInMilliseconds.Average();




            return result;
        }

        private List<Coord> GetSnappedSection(MapMatcher matcher, List<Coord> cleanShape, int startIndex, int breakIndex)
        {
            var sequenceSoFar = matcher.State.GetMostLikelySequence();
            var connectedSequence = PathFinding.DijstraConnectUpSequence(sequenceSoFar, matcher.Graph);
            return TrimRoadSequence(connectedSequence.Select(st => st.Geometry).ToList(), cleanShape[startIndex], cleanShape[breakIndex]);
        }
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

        //private List<Coord> Trim(List<DirectedRoad> roads)
        //{
        //    var result = roads.SelectMany(x => x.Geometry.Take(x.Geometry.Count - 1)).ToList();
        //    result.Add(roads.Last().Geometry.Last());

        //    return result;
        //}
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
    }
}