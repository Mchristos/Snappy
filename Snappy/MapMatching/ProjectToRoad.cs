using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;

namespace Snappy.MapMatching
{
    //Stores information for a co-ordinate projected onto a directed road.
    public class ProjectToRoad
    {
        // Distance from the co-ordinate to its projection on the road
        public Distance ProjectedDistance { get; set; }
        public Coord Coordinate { get; set; }
        public Coord Projection { get; set; }
        public DirectedRoad Road { get; set; }
        public int IndexInRoad { get; set; }

        public ProjectToRoad(Coord coord, DirectedRoad road)
        {
            Coordinate = coord;
            Road = road;

            int position;
            Coord projection = coord.SnapToPolyline(road.Geometry, out position);
            IndexInRoad = position;
            Projection = projection;
            ProjectedDistance = coord.HaversineDistance(projection);
        }        

        // On-road distance from the beginning of the road to the projected point.
        public Distance DistanceFromStart
        {
            get
            {
                return DistanceFunctions.ComputeCumulativeDistanceFromStart(Road.Geometry, IndexInRoad, Projection);
            }
        }
        public Distance DistanceToEnd
        {
            get
            {
                return DistanceFunctions.ComputeDistanceToEnd(Road.Geometry, IndexInRoad, Projection);
            }
        }
    }
}