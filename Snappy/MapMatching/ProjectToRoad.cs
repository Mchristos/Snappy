using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System.Linq;

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

            int position = -1;
            Coord projection = coord.SnapToPolyline(road.Geometry, out position);
            IndexInRoad = position;
            Projection = projection;
            ProjectedDistance = coord.HaversineDistance(projection);

            DistanceFromStart = computeDistanceFromStart();
            DistanceToEnd = computeDistanceToEnd();
        }        

        // On-road distance from the beginning of the road to the projected point.
        public Distance DistanceFromStart { get; set; }
        public Distance DistanceToEnd { get; set; }

        private Distance computeDistanceFromStart()
        {           
            return DistanceFunctions.ComputeCumulativeDistanceFromStart(Road.Geometry, IndexInRoad, Projection);            
        }
        private Distance computeDistanceToEnd()
        {            
            return DistanceFunctions.ComputeDistanceToEnd(Road.Geometry, IndexInRoad, Projection);            
        }
    }
}