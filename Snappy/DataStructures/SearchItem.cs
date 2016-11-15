using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.DataStructures
{
    public class SearchItem : IComparable<SearchItem>
    {
        public long Id;

        //remembers the road leading to this item (since different roads can come from the same previous node id) 
        public DirectedRoad PrevRoad { get; set; }
        public SearchItem Prev { get; set; }

        public double Distance { get; set; }

        public SearchItem(long id, DirectedRoad prevRoad, SearchItem prev, double dist)
        {
            //if(prevRoad.Start != prev.Id) { throw new ArgumentException("Not correct."); }
            Id = id;
            PrevRoad = prevRoad;
            Prev = prev;
            Distance = dist;
        }

        public int CompareTo(SearchItem other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}
