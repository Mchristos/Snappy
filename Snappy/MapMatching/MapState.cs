using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.MapMatching
{
    // Corresponds to a road in the network for map matching. Includes the user-specified data for this state / road. 
    public class MapState<T>
    {
        public DirectedRoad Road { get; set; }
        public T Datum { get; set; }

        public MapState(DirectedRoad road, T datum)
        {
            Road = road;
            Datum = datum;
        }
    }
}
