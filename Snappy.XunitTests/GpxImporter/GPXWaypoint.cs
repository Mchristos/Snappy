using System;

namespace Snappy.XunitTests.GpxImporter
{
    public class GPXWaypoint : GPXPoint
    {
        public String Name { get; set; }
        public GPXWaypoint(double lat, double lon, DateTime date, string name)
            : base(lat, lon, date)
        {
            Name = name;
        }
    }
}
