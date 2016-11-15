using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.XunitTests
{
    public static class Extensions
    {
        public static List<Coord> ToCoordList(this double[,] data)
        {
            var result = new List<Coord>();
            for (int i = 0; i < data.GetLength(0); i++)
            {
                result.Add(new Coord(data[i, 0], data[i, 1]));
            }
            return result;
        }
    }
}
