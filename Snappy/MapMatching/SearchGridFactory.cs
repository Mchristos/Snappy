using Snappy.Config;
using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.MapMatching
{
    public static class SearchGridFactory
    {
        public static RoadSearchGrid ComputeSearchGrid(RoadGraph graph, double gridSizeInMeters, BoundingBox boundingBox = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            double minLat = double.NaN;
            double maxLat = double.NaN;
            double minLng = double.NaN;
            double maxLng = double.NaN;
            if (boundingBox != null)
            {
                minLat = boundingBox.LatLowerBound;
                maxLat = boundingBox.LatUpperBound;
                minLng = boundingBox.LngLowerBound;
                maxLng = boundingBox.LngUpperBound;
            }
            else
            {
                List<Coord> allCoords = graph.Roads.SelectMany(x => x.Geometry).ToList();
                var latitudes = allCoords.Select(x => x.Latitude);
                var longitudes = allCoords.Select(x => x.Longitude);
                minLat = latitudes.Min();
                maxLat = latitudes.Max();
                minLng = longitudes.Min();
                maxLng = longitudes.Max();
            }

            // Get rough lat/lng delta values corresponding to the grid size
            double refLatInRadians = (((minLat + maxLat) / 2) * (Math.PI / 180.0));
            double roughLngGridSize = ((gridSizeInMeters / (Constants.Earth_Radius_In_Meters * Math.Cos(refLatInRadians))) * (180.0 / Math.PI));
            double roughLatGridSize = ((gridSizeInMeters / Constants.Earth_Radius_In_Meters) * (180.0 / Math.PI));

            // Get total width and height of the grid
            double lngWidth = (maxLng + roughLngGridSize) - (minLng - roughLngGridSize);
            double latHeight = (maxLat + roughLatGridSize) - (minLat - roughLatGridSize);

            // Corners are the minimum values with an added margin
            double left = minLng - roughLngGridSize;
            double bottom = minLat - roughLatGridSize;

            // Dimensions are the total length over the grid size ( may not divide in perfectly)
            int cellCountX = (int)(lngWidth / roughLngGridSize);
            int cellCountY = (int)(latHeight / roughLatGridSize);

            // Actual grid sizes are obtained from the above dimensions and the length the sides
            double gridSizeX = lngWidth / cellCountX;
            double gridSizeY = latHeight / cellCountY;

            var result = new RoadSearchGrid(left, bottom, gridSizeX, gridSizeY, cellCountX, cellCountY);
            // TODO: correct roads input below??
            result.Populate(graph.Values.SelectMany(x => x));

            stopwatch.Stop();
            return result;
        }
    }
}