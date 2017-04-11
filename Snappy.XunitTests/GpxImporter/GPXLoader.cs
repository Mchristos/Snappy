using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace Snappy.XunitTests.GpxImporter
{
    public static class GPXLoader
    {
        private static XNamespace GetGpxNameSpace()
        {
            XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");
            return gpx;
        }

        public static GPXData LoadGPXFile(string fileString)
        {
            XDocument gpxDoc = XDocument.Parse(fileString);
            XNamespace gpx = GetGpxNameSpace();

            var metaData = new GPXMeta(gpxDoc.Root.Attribute("creator").ToString(), gpxDoc.Root.Attribute("version").ToString());

            var waypoints = gpxDoc.Descendants(gpx + "wpt").Select(
                            waypoint => new GPXWaypoint(
                            Double.Parse(waypoint.Attribute("lat").Value),
                            Double.Parse(waypoint.Attribute("lon").Value),
                            DateTime.Parse(waypoint.Element(gpx + "time").Value),
                            waypoint.Element(gpx + "name") == null ? "" : waypoint.Element(gpx + "name").Value)
                            ).ToList();

            List<GPXTrackPoint> trackPoints = new List<GPXTrackPoint>();

            bool hasExtentions = gpxDoc.Descendants(gpx + "trkpt").Elements("extentions").Any();

            if (hasExtentions && gpxDoc.Descendants(gpx + "trkpt").Elements("extentions").Elements(gpx + "speed").Any())
            {
                trackPoints = gpxDoc.Descendants(gpx + "trkpt").Select(
                                trackpoint => new GPXTrackPoint(
                                Double.Parse(trackpoint.Attribute("lat").Value),
                                Double.Parse(trackpoint.Attribute("lon").Value),
                                DateTime.Parse(trackpoint.Element(gpx + "time").Value),
                                Double.Parse(trackpoint.Element(gpx + "extensions").Element(gpx + "speed").Value))
                                ).ToList();
            }
            else
            {
                // Added check to see if time is null for each point
                trackPoints = gpxDoc.Descendants(gpx + "trkpt").Select(
                                trackpoint => new GPXTrackPoint(
                                Double.Parse(trackpoint.Attribute("lat").Value),
                                Double.Parse(trackpoint.Attribute("lon").Value),
                                trackpoint.Element(gpx + "time") == null ? new DateTime() : DateTime.Parse(trackpoint.Element(gpx + "time").Value))
                                ).ToList();                
            }

            return new GPXData(metaData, waypoints, trackPoints);
        }
    }
}
