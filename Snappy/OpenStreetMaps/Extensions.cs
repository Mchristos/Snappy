using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.ValueObjects;

namespace Snappy.OpenStreetMaps
{
    public static class Extensions
    {


        public static Coord ToCoord(this OsmNode node)
        {
            return new Coord(node.Lat, node.Lon);
        }

        public static string ParseName(this Way way)
        {
            if (way.Tags.ContainsKey("name"))
            {
                return way.Tags["name"];
            }
            else
            {
                return "";
            }
        }
        public static bool IsOneWay(this Way way)
        {
            if (way.Tags.ContainsKey("oneway"))
            {
                return way.Tags["oneway"] == "yes";
            }
            else
            {
                return false;
            }
        }

    }
}
