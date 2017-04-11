using System;

namespace Snappy.XunitTests.GpxImporter
{
    public class GPXPoint
    {
        public GPXPoint(double lat, double lon, DateTime date)
        {
            Latitude = lat;
            Longitude = lon;
            Date = date;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
       // public double Elevation { get; set; }
        public DateTime Date { get; set; }
    }
}
