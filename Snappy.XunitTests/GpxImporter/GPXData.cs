using System.Collections.Generic;

namespace Snappy.XunitTests.GpxImporter
{
    public class GPXData
    {
        public IList<GPXWaypoint> WayPoints { get; private set; }

        public IList<GPXTrackPoint> TrackPoints { get; private set; }

        public GPXMeta MetaData { get; private set; }

        public GPXData(GPXMeta metadata, IList<GPXWaypoint> waypoints, IList<GPXTrackPoint> tracks)
        {
            MetaData = metadata;
            WayPoints = waypoints;
            TrackPoints = tracks;
        }

    }
}
