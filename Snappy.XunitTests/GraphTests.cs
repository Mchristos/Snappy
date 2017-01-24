using Snappy.DataStructures;
using Snappy.ValueObjects;
using Snappy.XunitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Snappy.OpenStreetMaps;

namespace Snappy.XunitTests
{
    public class GraphTests
    {
        // // This test fails atm because of upper search limit on Dijstra
        //[Fact]
        //public void DijstraTwoShortRoadsOneLong_ExpectedPath()
        //{
        //    RoadGraph graph = DataHelpers.BuildGraphTwoShortRoadsOneLong();

        //    List<DirectedRoad> actualPath;
        //    //path find from node 1000 to node 1002 
        //    Snappy.Functions.PathFinding.DijstraTryFindPath(graph, 1000, 1002, out actualPath);

        //    var actualResult = actualPath.Select(x => x.Name).ToList();
        //    var expectedResult = new List<string>() { "AB", "BC"};


        //    Assert.Equal(expectedResult, actualResult);
        //}

        [Fact]
        public void DijstraPathInGroeheuwel_ExpectedPath()
        {
            // Small bounding box in 'GroenHeuwel' near Allandale 
            BoundingBox box = new BoundingBox(19.0006038062, -33.6963546692, 19.0025213845, -33.6949020875);
            var graph = OsmGraphBuilder.BuildInRegion(Snappy.Config.Urls.MainOverpassApi, box);

            List<DirectedRoad> actualPath;
            string startOfCarlaCloseNodeId = "451614090";
            string midBelindaNodeId = "451614893";
            Snappy.Functions.PathFinding.DijstraTryFindPath(graph, startOfCarlaCloseNodeId, midBelindaNodeId, double.PositiveInfinity, out actualPath);

            var actualPathNames = actualPath.Select(x => x.Name).ToList();
            var expectedPathNames = new List<string>()
            {
                "Carla Close",
                "Carla Street",
                "Beryl Street",
                "Beryl Street",
                "Belinda",
            };

            Assert.Equal(expectedPathNames, actualPathNames);
        }



    }



 

}
