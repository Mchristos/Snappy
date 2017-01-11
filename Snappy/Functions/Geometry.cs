using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class Geometry
    {
        /// <summary>
        /// Projects a point onto a line segment, not taking into account the curvature of earth
        /// (assumes the points are in a 2d cartesian space).
        /// </summary>
        /// <param name="point"> Input point </param>
        /// <param name="segmentStart"> Start of segment to snap onto </param>
        /// <param name="segmentEnd"> End of segment to snap onto </param>
        /// <returns></returns>
        public static Coord SnapToSegment(this Coord point, Coord segmentStart, Coord segmentEnd)
        {
            if (segmentStart == segmentEnd) return segmentStart;

            var u = ((point.Latitude - segmentStart.Latitude) * (segmentEnd.Latitude - segmentStart.Latitude)) + ((point.Longitude - segmentStart.Longitude) * (segmentEnd.Longitude - segmentStart.Longitude));

            var udenominator = Math.Pow(segmentEnd.Latitude - segmentStart.Latitude, 2) + Math.Pow(segmentEnd.Longitude - segmentStart.Longitude, 2);

            u /= udenominator;

            var rLatitude = segmentStart.Latitude + (u * (segmentEnd.Latitude - segmentStart.Latitude));
            var rLongitude = segmentStart.Longitude + (u * (segmentEnd.Longitude - segmentStart.Longitude));
            var result = new Coord(rLatitude, rLongitude);
            var minx = Math.Min(segmentStart.Latitude, segmentEnd.Latitude);
            var maxx = Math.Max(segmentStart.Latitude, segmentEnd.Latitude);
            var miny = Math.Min(segmentStart.Longitude, segmentEnd.Longitude);
            var maxy = Math.Max(segmentStart.Longitude, segmentEnd.Longitude);
            var isValid = (result.Latitude >= minx && result.Latitude <= maxx) && (result.Longitude >= miny && result.Longitude <= maxy);
            if (isValid)
            {
                return result;
            }
            var distanceToStart = segmentStart.HaversineDistance(point);
            var distanceToEnd = segmentEnd.HaversineDistance(point);
            return (distanceToStart < distanceToEnd ? segmentStart : segmentEnd);
        }

        /// <summary>
        /// Projects a point onto a polyline, not taking into account the curvature of earth.
        /// (assumes the points are in a 2d cartesian space).
        /// </summary>
        /// <param name="point"> Input point </param>
        /// <param name="shape"> Polyline </param>
        /// <param name="position"> Zero-based index of the segment in the polyline that it got snapped onto (considering the polyline as a sequence of segments). </param>
        /// <returns></returns>
        public static Coord SnapToPolyline(this Coord point, List<Coord> shape, out int position)
        {
            double leastDistance = double.PositiveInfinity;
            int leastIndex = -1;
            List<Coord> projections = new List<Coord>();
            for (int i = 1; i < shape.Count; i++)
            {
                Coord projection = point.SnapToSegment(shape[i - 1], shape[i]);
                projections.Add(projection);
                double distance = point.HaversineDistance(projection).DistanceInMeters;
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    leastIndex = i - 1;
                }
            }

            if (leastIndex > -1)
            {
                position = leastIndex;
                return projections[position];
            }
            else throw new Exception("Failed to find the index of the closest projection to the stop");
        }
        /// <summary>
        /// Projects a point onto a polyline, not taking into account the curvature of earth.
        /// (assumes the points are in a 2d cartesian space).
        /// </summary>
        /// <param name="point"> Input point </param>
        /// <param name="shape"> Polyline </param>
        /// <param name="minIndex"> Only points beyond this index are considered </param>
        /// <param name="position"> Zero-based index of the segment in the polyline that it got snapped onto (considering the polyline as a sequence of segments). </param>
        /// <returns></returns>
        public static Coord SnapToPolyline(this Coord point, List<Coord> shape, int minIndex, out int position)
        {
            if (minIndex > shape.Count - 2) { throw new ArgumentException("minIndex must be less than the highest index into the shape"); }

            double leastDistance = double.PositiveInfinity;
            int leastIndex = -1;
            Coord leastProjection = null;
            for (int i = minIndex + 1; i < shape.Count; i++)
            {
                Coord projection = point.SnapToSegment(shape[i - 1], shape[i]);
                double distance = point.HaversineDistance(projection).DistanceInMeters;
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    leastIndex = i - 1;
                    leastProjection = projection;
                }
            }

            if (leastIndex > -1)
            {
                position = leastIndex;
                return leastProjection;
            }
            else throw new Exception("Failed to find the index of the closest projection to the stop");
        }


        public static List<Coord> TrimGeometryFrom(this List<Coord> road, Coord start)
        {
            int snapIndex;
            Coord projection = start.SnapToPolyline(road, out snapIndex);
            var result = new List<Coord>() { projection };
            result.AddRange(road.Skip(snapIndex + 1));
            return result;
        }

        public static List<Coord> TrimGeometryUntil(this List<Coord> road, Coord end)
        {
            int snapIndex;
            Coord projection = end.SnapToPolyline(road, out snapIndex);
            var result = road.Take(snapIndex + 1).ToList();
            result.Add(projection);
            return result;
        }

        public static List<Coord> TrimGeometryBetween(this List<Coord> road, Coord start, Coord end)
        {
            int startIndex;
            Coord startProjection = start.SnapToPolyline(road, out startIndex);

            int endIndex;
            Coord endProjection = end.SnapToPolyline(road, out endIndex);

            var result = new List<Coord>() { startProjection };
            int count = endIndex - startIndex - 1;
            if (count > 0)
            {
                List<Coord> middle = road.GetRange(startIndex + 1, count).ToList();
                result.AddRange(middle);
            }
            result.Add(endProjection);
            return result;
        }
    }
}