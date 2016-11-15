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

        

        public List<DirectedRoad> SnapDat(List<Coord> coords)
        {
            BoundingBox boundingBox = coords.GetBoundingBox(20);
            var osmGraph = OsmGraphBuilder.BuildInRegion(_overpassApi, boundingBox);
            var mapMatcher = new MapMatcher(osmGraph);
            foreach (var coord in coords)
            {
                mapMatcher.UpdateState(coord);
            }

            //get correct sequence of roads
            var sequence = mapMatcher.State.GetMostLikelySequence();
            //connect up sequence
            var connectedSequence = sequence;//.DijstraConnectUpSequence(osmGraph);

            return connectedSequence;
        }



    }
}
