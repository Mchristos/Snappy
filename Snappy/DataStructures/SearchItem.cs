using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.DataStructures
{
    public class SearchItem
    {
        public long Id { get; set; }

        public SearchItem Prev { get; set; }

        public double Distance { get; set; }

        public SearchItem(long id, SearchItem prev, double dist)
        {
            Id = id;
            Prev = prev;
            Distance = dist;
        }

    }
}
