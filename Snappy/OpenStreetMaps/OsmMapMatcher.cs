using Snappy.Config;
using Snappy.DataStructures;
using Snappy.MapMatching;

namespace Snappy.OpenStreetMaps
{
    public class OsmMapMatcher : MapMatcher<DirectedRoad>
    {
        public OsmMapMatcher(RoadGraph graph)
        {
            SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, Constants.Search_Grid_Grid_Size_In_Meters);
            Graph = graph;
            State = MapMatchState.InitialState();
        }

        private OsmMapMatcher()
        {
        }

        public OsmMapMatcher Clone()
        {
            OsmMapMatcher result = new OsmMapMatcher();
            result.Graph = Graph;
            result.SearchGrid = SearchGrid.Clone();
            result.State = MapMatchState.InitialState();
            return result;
        }
    }
}