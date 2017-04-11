using System;

namespace Snappy.XunitTests.GpxImporter
{
    public class GPXTrackPoint : GPXPoint
    {
        public bool HasSpeed { get { return Speed != -1; } }
        public double Speed { get; set; }

        public GPXTrackPoint(double lat, double lon, DateTime date, double speed)
            : base(lat, lon, date)
        {
            Speed = speed;
        }

        public GPXTrackPoint(double lat, double lon, DateTime date)
            : base(lat, lon, date)
        {
            Speed = -1;
        }
    }
}
