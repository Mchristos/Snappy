using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Functions;
using Xunit;

namespace Snappy.XunitTests
{
    public class ExtensionsTests
    {

        [Fact]
        public void GetSurroudingCells_Expected1()
        {
            var cell = new int[] { 0, 0 };
            var result = cell.GetSurroundingCells(5, 5);

            var expectedCell1 = new int[] { 0 , 0 };
            var expectedCell2 = new int[] { 0 , 1 };
            var expectedCell3 = new int[] { 1 , 0 };
            var expectedCell4 = new int[] { 1 , 1 };

            Assert.Contains(expectedCell1, result);
            Assert.Contains(expectedCell2, result);
            Assert.Contains(expectedCell3, result);
            Assert.Contains(expectedCell4, result);
        }


        [Fact]
        public void GetSurroudingCells_Expected2()
        {
            var cell = new int[] {4,4 };
            var result = cell.GetSurroundingCells(5, 5);

            var expectedCell1 = new int[] { 4,4 };
            var expectedCell2 = new int[] { 3,4 };
            var expectedCell3 = new int[] { 4,3 };
            var expectedCell4 = new int[] { 3,3 };

            Assert.Contains(expectedCell1, result);
            Assert.Contains(expectedCell2, result);
            Assert.Contains(expectedCell3, result);
            Assert.Contains(expectedCell4, result);
        }

        [Fact]
        public void InclusivePartitionWithPredicate_Expected()
        {
            // Test 1 
            var input1 = new List<int>() { 1, 0, 0, 1, 0, 0, 1};
            var result1 = input1.InclusivePartitioning(x => x == 1);
            var onezerozeroone = new List<int>() { 1,0,0,1};
            var expected1 = new List<List<int>>() { onezerozeroone, onezerozeroone };

            Assert.Equal(result1, expected1);
        }

    }
}
