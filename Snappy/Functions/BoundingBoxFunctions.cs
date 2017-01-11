using Snappy.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class BoundingBoxFunctions
    {
        /// <summary>
        /// Gets bounding box bounding a set of co-ordinates
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public static BoundingBox GetBoundingBox(this IEnumerable<Coord> coordinates)
        {
            var lats = coordinates.Select(x => x.Latitude);
            var lngs = coordinates.Select(x => x.Longitude);
            return new BoundingBox(lngs.Min(), lats.Min(), lngs.Max(), lats.Max());
        }

        public static BoundingBox GetBoundingBox(this IEnumerable<Coord> coordinates, double marginInMeters)
        {
            var lats = coordinates.Select(x => x.Latitude);
            var lngs = coordinates.Select(x => x.Longitude);
            var latMargin = MathExtensions.MetersToDeltaLat(marginInMeters);
            //random reference latitude!
            var randomLat = coordinates.First().Latitude;
            var lngMargin = MathExtensions.MetersToDeltaLng(marginInMeters, randomLat);

            return new BoundingBox(lngs.Min() - lngMargin, lats.Min() - latMargin, lngs.Max() + lngMargin, lats.Max() + latMargin);
        }

        /// <summary>
        /// Groups together overlapping boundingBoxes
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public static List<List<BoundingBox>> GroupConnectedBoxes(this IEnumerable<BoundingBox> boxes)
        {
            var result = new List<List<BoundingBox>>();
            var boxSet = new HashSet<BoundingBox>(boxes);
            while (boxSet.Count > 0)
            {
                var singleton = new HashSet<BoundingBox> { boxSet.First() };
                var closure = singleton.Closure(boxSet, (x, y) => x.IsConnectedTo(y));
                result.Add(closure.ToList());
                boxSet = new HashSet<BoundingBox>(boxSet.Except(closure));
            }
            return result;
        }

        /// <summary>
        /// Determines whether two bounding boxes overlap
        /// </summary>
        /// <param name="box"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static bool IsConnectedTo(this BoundingBox box, BoundingBox other)
        {
            //lng interval must overlap && lats must overlap
            bool result = true;
            result = result && !(other.LngUpperBound < box.LngLowerBound && other.LngLowerBound < box.LngLowerBound);

            result = result && !(box.LngUpperBound < other.LngLowerBound && box.LngUpperBound < other.LngUpperBound);

            result = result && !(other.LatUpperBound < box.LatLowerBound && other.LatLowerBound < box.LatLowerBound);

            result = result && !(box.LatUpperBound < other.LatLowerBound && box.LatUpperBound < other.LatUpperBound);

            return result;
        }

        public static BoundingBox Merge(this IEnumerable<BoundingBox> boxes)
        {
            var lowerLat = boxes.Select(x => x.LatLowerBound).Min();
            var lowerLng = boxes.Select(x => x.LngLowerBound).Min();
            var upperLat = boxes.Select(x => x.LatUpperBound).Max();
            var upperLng = boxes.Select(x => x.LngUpperBound).Max();
            return new BoundingBox(lowerLng, lowerLat, upperLng, upperLat);
        }

        /// <summary>
        /// Determines if a bounding box contains another bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="otherBox"></param>
        /// <returns></returns>
        public static bool Contains(this BoundingBox box, BoundingBox otherBox)
        {
            return (box.LatLowerBound <= otherBox.LatLowerBound) && (box.LatUpperBound >= otherBox.LatUpperBound)
                    && (box.LngLowerBound <= otherBox.LngLowerBound) && (box.LngUpperBound >= otherBox.LngUpperBound);
        }

        public static bool Contains(this BoundingBox box, Coord point)
        {
            return (box.LatLowerBound < point.Latitude && point.Latitude < box.LatUpperBound
                 && box.LngLowerBound < point.Longitude && point.Longitude < box.LngUpperBound);
        }
    }
}