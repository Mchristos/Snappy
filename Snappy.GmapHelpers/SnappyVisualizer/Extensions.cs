using GMap.NET;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.GmapHelpers
{
    public static class Extensions
    {

        public static PointLatLng ToPointLatLng(this Coord coord)
        {
            return new PointLatLng(coord.Latitude, coord.Longitude);
        }


        public static Coord ToCoord(this PointLatLng point)
        {
            return new Coord(point.Lat, point.Lng);
        }

    }
}
