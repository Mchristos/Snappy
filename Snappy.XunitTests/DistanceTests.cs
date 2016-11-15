using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Snappy.XunitTests
{
    public class DistanceTests
    {

        [Theory]
        [InlineData(-33.5,18.0, -33.8, 18.0, 33.3585)]
        [InlineData(-33.942116, 18.432849, -33.943253, 18.443574, 0.99739)]
        public void HaversineDistance_Expected(double lat1, double lng1, double lat2, double lng2, double expectedDistanceInKilometres)
        {
            Coord start = new Coord(lat1, lng1);
            Coord end = new Coord(lat2, lng2);

            Distance actualResult = start.HaversineDistance(end);

            Assert.Equal(expectedDistanceInKilometres, actualResult.DistanceInKilometers, 4);
        }


    }
}
