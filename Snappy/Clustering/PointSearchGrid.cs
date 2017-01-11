using Snappy.DataStructures;
using Snappy.ValueObjects;

namespace Snappy.Clustering
{
    public class PointSearchGrid : SearchGrid<Coord>
    {
        public PointSearchGrid(double left, double bottom, double gridSizeX, double gridSizeY, int cellCountX, int cellCountY) : base(left, bottom, gridSizeX, gridSizeY, cellCountX, cellCountY)
        {
        }

        public void Populate()
        {
        }
    }
}