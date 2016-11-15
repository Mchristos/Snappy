using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Snappy.DataStructures;

namespace Snappy.XunitTests
{
    public class SearchGridTests
    {
        [Theory]
        [InlineData(0,0,3.33, 3.33, 5, 5, 4.22, 2.22, 1,0)]
        public void GridCellOfPoint_Expected(double left, double bottom, double gridSizeX, double gridSizeY, int cellCountX, int cellCountY, double queryPointX, double queryPointY, int expectedCellX, int expectedCellY)
        {
            var grid = new SearchGrid<string>(left, bottom, gridSizeX, gridSizeY, cellCountX, cellCountY);
            var resultCell = grid.GetGridCellOfPoint(queryPointX, queryPointY);
            Assert.Equal(expectedCellX, resultCell[0]);
            Assert.Equal(expectedCellY, resultCell[1]);
        }
    }
}
