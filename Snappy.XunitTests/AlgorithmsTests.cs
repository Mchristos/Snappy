using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Snappy.Functions;

namespace Snappy.XunitTests
{
    public class AlgorithmsTests
    {

        [Fact]
        public void RepeatedIdsTest1_Expected()
        {
            var input = new List<long[]>()
            {
                new long[] {12, 24, 132, 31213},
                new long[] {24, 12, 122},
            };
            var expectedResult = new Dictionary<long, int>();
            expectedResult[12] = 2;
            expectedResult[24] = 2;
            expectedResult[132] = 1;
            expectedResult[122] = 1;
            expectedResult[31213] = 1;
            var actual = input.CountRepeatedIds();
            Assert.Equal(expectedResult, actual);
        }


        [Fact]
        public void RepeatedIdsTest2_Expected()
        {
            var input = new List<long[]>()
            {
                new long[] {123456789},
                new long[] {123456789},
                new long[] {123456789},
                new long[] {123456789},
            };
            var expectedResult = new Dictionary<long, int>();
            expectedResult[123456789] = 4;
            var actual = input.CountRepeatedIds();
            Assert.Equal(expectedResult, actual);
        }


        [Fact]
        public void TraceBackSequence_Expected()
        {
            var input = new List<Dictionary<int, int>>();
            input.Add(
                new Dictionary<int, int>()
                {
                    [1] = 1,
                    [2] = 2,
                    [3] = 3                
                });

            int seedValue = 1;
            var actualResult = input.TraceBackSequence(seedValue);
            // expect traced back sequnce to be one longer than the input 'memory' list 
            Assert.Equal(input.Count + 1, actualResult.Count);

            var expectedResult = new List<int>() { 1, 1 };

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void TraceBackSequence_Exception()
        {
            var input = new List<Dictionary<int, int>>();
            input.Add(
                new Dictionary<int, int>()
                {
                    [1] = 1,
                    [2] = 2,
                    [3] = 3
                });

            // seed value to start tracing back sequence not valid
            int seedValue = 99;
            Assert.Throws<ArgumentException>(() => input.TraceBackSequence(seedValue) );
        }


    }
}
