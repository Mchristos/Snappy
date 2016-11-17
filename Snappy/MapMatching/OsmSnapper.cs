using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snappy.Enums;
using Snappy.DataStructures;
using Snappy.ValueObjects;
using Snappy.Functions;
using Snappy.OpenStreetMaps;

namespace Snappy.MapMatching
{
    public class OsmSnapper
    {

        private string _overpassApi { get; set; }
        public OsmSnapper(OverpassApi overpassApi)
        {
            if (overpassApi == OverpassApi.LocalDelorean) _overpassApi = Config.Urls.DeloreanGray;
            else if (overpassApi == OverpassApi.MainOverpass) _overpassApi = Config.Urls.MainOverpassApi;
            else throw new ArgumentException("Invalid overpass enum"); 
        }        

        

        public List<Coord> SnapDat(List<Coord> coords)
        {
            var totalTimeStopwatch = new System.Diagnostics.Stopwatch();
            totalTimeStopwatch.Start();

            BoundingBox boundingBox = coords.GetBoundingBox(20);
            var osmGraph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox);
            var mapMatcher = new MapMatcher(osmGraph);

            var snapStopwatch = new System.Diagnostics.Stopwatch();
            snapStopwatch.Start();
            foreach (var coord in coords.GetCleanedCoordinates())
            {
                mapMatcher.UpdateState(coord);
            }
            //get correct sequence of roads
            var sequence = mapMatcher.State.GetMostLikelySequence();
            //connect up sequence
            var connectedSequence = sequence.DijstraConnectUpSequence(osmGraph);
            var result = Trim(connectedSequence);
            snapStopwatch.Stop();
            Console.WriteLine(snapStopwatch.Elapsed.TotalSeconds + " seconds to perform snapping");

            totalTimeStopwatch.Stop();
            Console.WriteLine("Total snap time: " + totalTimeStopwatch.Elapsed.TotalSeconds + " seconds");
            return result;
        }



        private List<Coord> Trim(List<DirectedRoad> roads)
        {
            var result = roads.SelectMany(x => x.Geometry.Take(x.Geometry.Count - 1)).ToList();
            result.Add(roads.Last().Geometry.Last());

            return result;
        }



    }
}
