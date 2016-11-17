using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Functions;

namespace Snappy.ValueObjects
{
    public class BoundingBox
    {
        public double LatLowerBound { get; set; }
        public double LatUpperBound { get; set; }
        public double LngLowerBound { get; set; }
        public double LngUpperBound { get; set; }

        public BoundingBox(double west, double south, double east, double north)
        {
            if (!Extensions.IsValidLngInDegrees(west)  || !Extensions.IsValidLngInDegrees(east)) { throw new ArgumentException("Invalid longitude"); }
            if (!Extensions.IsValidLatInDegrees(south) || !Extensions.IsValidLatInDegrees(north)) { throw new ArgumentException("Invalid latitude"); }
            if(!(west <= east) || !(south <= north)) { throw new ArgumentException("Invalid bounding box"); }

            LngLowerBound = west;
            LatLowerBound = south;
            LngUpperBound = east;
            LatUpperBound = north;
        }



        
        public Coord Center
        {
            get
            {
                return new Coord((LatLowerBound + LatUpperBound) / 2, (LngLowerBound + LngUpperBound) / 2);
            }                
        }
    }
}
