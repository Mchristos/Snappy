using System;

namespace Snappy.DataStructures
{
    public class DijstraSearchItem : IComparable<DijstraSearchItem>
    {
        public string Id;

        //remembers the road leading to this item (since different roads can come from the same previous node id)
        public DirectedRoad PrevRoad { get; set; }

        public DijstraSearchItem Prev { get; set; }

        public double Distance { get; set; }

        public DijstraSearchItem(string id, DirectedRoad prevRoad, DijstraSearchItem prev, double dist)
        {
            //if(prevRoad.Start != prev.Id) { throw new ArgumentException("Not correct."); }
            Id = id;
            PrevRoad = prevRoad;
            Prev = prev;
            Distance = dist;
        }

        public int CompareTo(DijstraSearchItem other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}