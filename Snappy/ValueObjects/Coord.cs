using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.ValueObjects
{
    public class Coord
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Coord(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }
}
