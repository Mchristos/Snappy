using Snappy.Config;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.Functions
{
    public static class CoordinateFunctions
    {
        /// <summary>
        /// "cleans" a co-ordinate track (potentially with associated time stamps) by only including subsequent co-ordinate if they fall
        /// sufficiently far (two standard deviations) from the last co-ordinate.
        /// </summary>
        /// <param name ="track"></param>
        /// <param name ="indices"></param>
        /// <param name ="timeStamps"></param>
        /// <param name ="radiusInMeters"></param>
        /// <returns></returns>
        public static List<Coord> GetCleanedCoordinates(this List<Coord> track, List<DateTime> timeStamps = null, double radiusInMeters = 2 * Constants.GPS_Error_In_Meters)
        {
            if (timeStamps != null)
            {
                if (track.Count != timeStamps.Count) throw new ArgumentException("Time stamps must correspond to trace co-ordinates");
            }
            if (track.Count < 2) return track;

            var result = new List<Coord>();
            result.Add(track.First());
            int currentIndex = 0;
            int iterator = 1;
            while (currentIndex < track.Count && iterator < track.Count)
            {
                ValueObjects.Distance dist = track[iterator].HaversineDistance(track[currentIndex]);
                bool passedSpeedTest = true;
                if (timeStamps != null)
                {
                    TimeSpan timeDiff = timeStamps[iterator] - timeStamps[currentIndex];
                    var avgSpeedInKmPerHour = dist.DistanceInKilometers / timeDiff.TotalHours;
                    passedSpeedTest = avgSpeedInKmPerHour < Constants.Too_Fast_In_Kilometres_Per_Hour_For_Coordinate_Cleaning;
                }
                if (dist.DistanceInMeters > radiusInMeters && passedSpeedTest)
                {
                    result.Add(track[iterator]);
                    currentIndex = iterator;
                }
                iterator += 1;
            }
            return result;
        }

        /// <summary>
        /// Replaces a polyline with an approximating polyline with less points. Intuitively, it takes jagged lines and makes them straight.
        /// </summary>
        /// <param name="polyline"> List of points defining the polyline </param>
        /// <param name="epsilon"> Margin of error in meters. No point in the input polyline will be further than epsilon away from the resulting output polyline.
        /// </param>
        /// <returns> List of points defining the approximating polyline.  </returns>
        public static List<Coord> DouglasPeucker(this List<Coord> polyline, double epsilon)
        {
            if (polyline.Count <= 2)
            {
                return polyline;
            }
            Coord start = polyline.First();
            Coord end = polyline.Last();
            double maxdist = 0;
            int maxindex = new int();
            for (int i = 1; i < polyline.Count - 1; i++)
            {
                Coord point = polyline[i];
                Coord projection = point.SnapToSegment(start, end);
                double dist = point.HaversineDistance(projection).DistanceInMeters;
                if (dist > maxdist)
                {
                    maxdist = dist;
                    maxindex = i;
                }
            }
            if (maxdist < epsilon)
            {
                return new List<Coord> { start, end };
            }
            else
            {
                List<Coord> first = polyline.Take(maxindex + 1).ToList();
                List<Coord> tail = polyline.GetRange(maxindex, polyline.Count - maxindex);
                return DouglasPeucker(first, epsilon).Concat(DouglasPeucker(tail, epsilon).GetRange(1, DouglasPeucker(tail, epsilon).Count - 1)).ToList();
            }
        }
        public static Snappy.ValueObjects.Coord ComputeCenter(this IEnumerable<Coord> coords)
        {
            var meanLat = coords.Select(x => x.Latitude).Average();
            var meanLng = coords.Select(x => x.Longitude).Average();
            return new Snappy.ValueObjects.Coord(meanLat, meanLng);
        }

        public static List<Coord> RemoveExactDuplicates(this List<Coord> input)
        {
            if (input.Count < 2) return input;
            var result = new List<Coord>() { input.First()};
            for (int i = 1; i < input.Count; i++)
            {
                Coord lastCoord = input[i - 1];
                Coord thisCoord = input[i];
                if(thisCoord.Latitude != lastCoord.Latitude || thisCoord.Longitude != lastCoord.Longitude)
                {
                    result.Add(thisCoord);
                }
            }
            return result;
        }

    }
}