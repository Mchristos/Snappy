using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.ValueObjects;
using Snappy.Functions;
using Snappy.Config;

namespace Snappy.MapMatching
{
    public static class SearchGridFactory
    {

        public static RoadSearchGrid ComputeSearchGrid(RoadGraph graph, double gridSizeInMeters)
        {
            List<Coord> allCoords = graph.Roads.SelectMany(x => x.Geometry).ToList();

            var latitudes = allCoords.Select(x => x.Latitude);
            var longitudes = allCoords.Select(x => x.Longitude);

            var minLat = latitudes.Min();
            var maxLat = latitudes.Max();
            var minLng  = longitudes.Min();
            var maxLng = longitudes.Max();

            // Get rough lat/lng delta values corresponding to the grid size
            double refLatInRadians = (((minLat + maxLat) / 2) * (Math.PI / 180.0));
            double roughLngGridSize = ((gridSizeInMeters / (Constants.Earth_Radius_In_Meters * Math.Cos(refLatInRadians))) * (180.0 / Math.PI));
            double roughLatGridSize = ((gridSizeInMeters / Constants.Earth_Radius_In_Meters) * (180.0 / Math.PI));
            // Get total width and height of the grid
            double lngWidth = (maxLng + roughLngGridSize) - (minLng - roughLngGridSize);
            double latHeight = (maxLat + roughLatGridSize) - (minLat - roughLatGridSize);


            // corners are the minimum values with an added margin
            double left   = minLng - roughLngGridSize;
            double bottom = minLat - roughLatGridSize;
            // dimensions are the total lenth over the grid size ( may not divide in perfectly)
            int cellCountX =  (int)  ( lngWidth / roughLngGridSize );
            int cellCountY =  (int)  ( latHeight / roughLatGridSize );
            // actual grid sizes are obtained from the above dimensions and the length the sides 
            double gridSizeX = lngWidth  / cellCountX;
            double gridSizeY = latHeight / cellCountY;

            var result = new RoadSearchGrid(left, bottom, gridSizeX, gridSizeY, cellCountX, cellCountY);
            // TODO: correct roads input below??
            result.Populate(graph.Values.SelectMany(x => x));         
            return result;
        }


    }
}
