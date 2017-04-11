using Snappy.OpenStreetMaps;
using Snappy.ValueObjects;
using Snappy.XunitTests.GpxImporter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Snappy.XunitTests
{
    public class SnappingTests
    {
        [Fact]
        public void SnapTest()
        {
            var gpxFolderPath = HelpfulThings.AssemblyDirectory + "\\TestData\\GpxTracks";
            DirectoryInfo d = new DirectoryInfo(gpxFolderPath);
            FileInfo[] files = d.GetFiles("*.gpx");
            OsmSnapper snapper = new OsmSnapper(Enums.OverpassApi.DeloreanGray);
            foreach (var file in files)
            {
                string fileString = new StreamReader(file.FullName).ReadToEnd();
                GPXData gpx = GPXLoader.LoadGPXFile(fileString);
                var track = gpx.TrackPoints.Select(x => new Coord(x.Latitude, x.Longitude));
                var snapped = snapper.SnapDat(track.ToList());
                // Assert the map matcher did not break when snapping this track, i.e. did not return a "disconnected" geometry
                Assert.Equal(1, snapped.Count);                
            }
        }
    }
}
