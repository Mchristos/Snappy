using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Snappy.DataStructures;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.GmapHelpers
{
    public static class PlottingHelper
    {

        /***************** plotting functionality *****************/

        // These functions add markers/polygons to a specified GMapOverlay.
        // They do not add the specified overlay to the map, this must be done after calling the plot function.

        public static void PlotArrayAsPoints(double[,] dataArray, GMarkerGoogleType type, GMapOverlay overlay)
        {
            int numberOfPoints = dataArray.GetLength(0);
            for (int i = 0; i < numberOfPoints; i++)
            {
                PointLatLng point = new PointLatLng(dataArray[i, 0], dataArray[i, 1]);
                GMarkerGoogle marker = new GMarkerGoogle(point, type);
                overlay.Markers.Add(marker);
            }
        }

        public static void PlotPoints(IEnumerable<Coord> coordinates, GMarkerGoogleType type, GMapOverlay overlay)
        {
            var coords = coordinates.ToArray();
            for (int i = 0; i < coords.Count(); i++)
            {
                PointLatLng point = new PointLatLng(coords[i].Latitude, coords[i].Longitude);
                GMarkerGoogle marker = new GMarkerGoogle(point, type);
                overlay.Markers.Add(marker);
            }
        }
        public static void PlotArrayAsPoints(Coord[] dataArray, GMarkerGoogleType type, GMapOverlay overlay)
        {
            int numberOfPoints = dataArray.GetLength(0);
            for (int i = 0; i < numberOfPoints; i++)
            {
                PointLatLng point = new PointLatLng(dataArray[i].Latitude, dataArray[i].Longitude);
                GMarkerGoogle marker = new GMarkerGoogle(point, type);
                overlay.Markers.Add(marker);
            }
        }
        public static void PlotArrayAsPoints(Coord[] dataArray, Bitmap markerPicture, GMapOverlay overlay)
        {
            int numberOfPoints = dataArray.GetLength(0);
            for (int i = 0; i < numberOfPoints; i++)
            {
                PointLatLng point = new PointLatLng(dataArray[i].Latitude, dataArray[i].Longitude);
                GMarkerGoogle marker = new GMarkerGoogle(point, markerPicture);
                overlay.Markers.Add(marker);
            }
        }

        //PlotPoint adds a marker of the specified type to the specified overlay. The overlay itself is not added to map!
        public static GMarkerGoogle PlotPoint(double latitude, double longitude, GMarkerGoogleType type, GMapOverlay overlay)
        {
            PointLatLng point = new PointLatLng(latitude, longitude);
            return PlotPoint(point, type, overlay);
        }
        public static GMarkerGoogle PlotPoint(double latitude, double longitude, GMarkerGoogleType type, GMapOverlay overlay, string toolTipText = "")
        {
            PointLatLng point = new PointLatLng(latitude, longitude);
            return PlotPoint(point, type, overlay, toolTipText);
        }
        public static GMarkerGoogle PlotPoint(Coord coord, GMarkerGoogleType type, GMapOverlay overlay)
        {
            PointLatLng point = new PointLatLng(coord.Latitude, coord.Longitude);
            return PlotPoint(point, type, overlay);
        }
        public static GMarkerGoogle PlotPoint(Coord coord, Bitmap pic, GMapOverlay overlay)
        {
            PointLatLng point = new PointLatLng(coord.Latitude, coord.Longitude);
            GMarkerGoogle marker = new GMarkerGoogle(point, pic);
            overlay.Markers.Add(marker);
            return marker;
        }

        public static GMarkerGoogle PlotPoint(PointLatLng point, GMarkerGoogleType type, GMapOverlay overlay, string toolTipText = null)
        {
            GMarkerGoogle marker = new GMarkerGoogle(point, type);
            if (toolTipText != null)
            {
                marker.ToolTipText = toolTipText;
            }
            overlay.Markers.Add(marker);
            return marker;
        }

        //Adds set of straight line (as polygons) to the specified overlay
        public static void PlotArrayAsLines(double[,] dataArray, Color color, GMapOverlay overlay)
        {
            for (int i = 1; i < dataArray.GetLength(0); i++)
            {
                PointLatLng start = new PointLatLng(dataArray[i - 1, 0], dataArray[i - 1, 1]);
                PointLatLng end = new PointLatLng(dataArray[i, 0], dataArray[i, 1]);
                List<PointLatLng> twoPointList = new List<PointLatLng>();
                twoPointList.Add(start);
                twoPointList.Add(end);
                GMapPolygon polygon = new GMapPolygon(twoPointList, "pol");
                polygon.Stroke = new Pen(color, 3);
                overlay.Polygons.Add(polygon);
            }
        }

        public static void PlotLine(Coord start, Coord end, GMapOverlay overlay, Color color, int width = 3)
        {
            PointLatLng s = new PointLatLng(start.Latitude, start.Longitude);
            PointLatLng e = new PointLatLng(end.Latitude, end.Longitude);
            List<PointLatLng> twoPointList = new List<PointLatLng>();
            twoPointList.Add(s);
            twoPointList.Add(e);
            GMapPolygon polygon = new GMapPolygon(twoPointList, "pol");
            polygon.Stroke = new Pen(color, width);
            overlay.Polygons.Add(polygon);
        }

        public static void PlotPolyLine(List<Coord> polyline, GMapOverlay overlay, Color color, int width = 3, bool highlightEnds = false)
        {
            for (int i = 1; i < polyline.Count; i++)
            {
                PlotLine(polyline[i - 1], polyline[i], overlay, color, width);
            }
            if (highlightEnds)
            {
                PlotPoint(polyline.First().Latitude, polyline.First().Longitude, GMarkerGoogleType.green_dot, overlay);
                PlotPoint(polyline.Last().Latitude, polyline.Last().Longitude, GMarkerGoogleType.green_dot, overlay);
            }
        }

        public static void PlotPolyLines(List<List<Coord>> lines, GMapOverlay overlay)
        {
            var colorarray = new Color[] { Color.Blue, Color.Green, Color.Red, Color.SaddleBrown, Color.Black, Color.Purple,
                                            Color.Yellow, Color.Turquoise, Color.DarkGreen , Color.DarkMagenta, Color.OrangeRed};
            for (int i = 0; i < lines.Count; i++)
            {
                var color = colorarray[i % colorarray.Length];
                PlotPolyLine(lines[i], overlay, color);
            }
        }

        public static void PlotPolygon(List<Coord> polygon, GMapOverlay overlay, Color color)
        {
            var poly = new List<PointLatLng>();
            foreach (var point in polygon)
            {
                poly.Add(new PointLatLng(point.Latitude, point.Longitude));
            }
            GMapPolygon gPolygon = new GMapPolygon(poly, "pol");
            gPolygon.Stroke = new Pen(color, 3);
            overlay.Polygons.Add(gPolygon);
        }

        public static void PlotPolygons(List<List<Coord>> polygons, GMapOverlay overlay)
        {
            var colorarray = new Color[] { Color.Blue, Color.Green, Color.Red, Color.SaddleBrown, Color.Black, Color.Purple,
                                            Color.Yellow, Color.Turquoise, Color.DarkGreen , Color.DarkMagenta, Color.OrangeRed};
            for (int i = 0; i < polygons.Count; i++)
            {
                var color = colorarray[i % colorarray.Length];
                PlotPolygon(polygons[i], overlay, color);
            }
        }



        //public static void PlotGraph(RoadGraph graph, GMapOverlay overlay, Color color)
        //{
        //    foreach (var intersection in graph.Values)
        //    {
        //        //PlotPoint(nodeCoord.Latitude, nodeCoord.Longitude, GMarkerGoogleType.blue_dot,overlay);
        //        foreach (var link in intersection.Links)
        //        {
        //            PlotPolyLine(link, overlay, color, 3, true);
        //        }
        //    }
        //}
        public static void PlotGraph(RoadGraph graph, GMapOverlay overlay, Color color)
        {
            foreach (var road in graph.Roads)
            {
                PlotRoute(road.Geometry, overlay, color);
            }
        }

        public static void PlotRoute(List<Coord> geometry, GMapOverlay overlay, Color color = default(Color), int width = 3, int opacity = 255)
        {
            GMapRoute root = new GMapRoute(geometry.Select(x => x.ToPointLatLng()), "");

            Pen stroke = (Pen)root.Stroke.Clone();
            if (color != default(Color))
            {
                stroke.Color = Color.FromArgb(opacity, color);
            }
            stroke.Width = width;
            root.Stroke = stroke;
            overlay.Routes.Add(root);
        }


        public static void PlotGrid<T>(SearchGrid<T> grid, GMapOverlay overlay, Color color = default(Color), int width = 3, int opacity = 255)
        {
            double bottomLat = grid.Bottom;
            double topLat = grid.Bottom + grid.CellCountY * grid.GridSizeY;
            double lng = grid.Left;
            for (int i = 0; i < grid.CellCountX; i++)
            {
                var lineStart = new PointLatLng(bottomLat, lng);
                var lineEnd = new PointLatLng(topLat, lng);
                GMapRoute root = new GMapRoute(new List<PointLatLng> { lineStart, lineEnd }, "");
                Pen stroke = (Pen)root.Stroke.Clone();
                if (color != default(Color))
                {
                    stroke.Color = Color.FromArgb(opacity, color);
                }
                stroke.Width = width;
                root.Stroke = stroke;

                overlay.Routes.Add(root);
                lng = lng + grid.GridSizeX;
            }


            double leftLng = grid.Left;
            double rightLng = grid.Left + grid.CellCountX * grid.GridSizeX;
            double lat = grid.Bottom;
            for (int i = 0; i < grid.CellCountY; i++)
            {
                var lineStart = new PointLatLng(lat, leftLng);
                var lineEnd = new PointLatLng(lat, rightLng);
                GMapRoute root = new GMapRoute(new List<PointLatLng> { lineStart, lineEnd }, "");
                Pen stroke = (Pen)root.Stroke.Clone();
                if (color != default(Color))
                {
                    stroke.Color = Color.FromArgb(opacity, color);
                }
                stroke.Width = width;
                root.Stroke = stroke;

                overlay.Routes.Add(root);
                lat = lat + grid.GridSizeY;
            }
        }


        public static void PlotBoundingBox(BoundingBox box, GMapOverlay overlay, Color color = default(Color), int width = 3)
        {
            List<GMapPolygon> polygons = new List<GMapPolygon>();

            List<Coord> left = new List<Coord> { box.BottomLeft, box.TopLeft };
            GMapPolygon leftSide = new GMapPolygon(left.Select(x => x.ToPointLatLng()).ToList(), "");
            polygons.Add(leftSide);


            List<Coord> top = new List<Coord> { box.TopLeft, box.TopRight };
            var topSide = new GMapPolygon(top.Select(x => x.ToPointLatLng()).ToList(), "");
            polygons.Add(topSide);

            var right = new List<PointLatLng> { box.TopRight.ToPointLatLng(), box.BottomRight.ToPointLatLng() };
            var rightSide = new GMapPolygon(right, "");
            polygons.Add(rightSide);

            var bottom = new List<PointLatLng> { box.BottomRight.ToPointLatLng(), box.BottomLeft.ToPointLatLng() };
            var bottomSide = new GMapPolygon(bottom, "");
            polygons.Add(bottomSide);

            foreach (var polygon in polygons)
            {
                Pen stroke = (Pen)polygon.Stroke.Clone();
                if (color != default(Color))
                {
                    stroke.Color = color;
                }
                stroke.Width = width;
                polygon.Stroke = stroke;

                overlay.Polygons.Add(polygon);
            }

        }
    }
}
