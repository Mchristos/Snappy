using Snappy.Config;
using Snappy.DataStructures;
using Snappy.MapMatching;

namespace Snappy.OpenStreetMaps
{
    public class OsmMapMatcher : MapMatcher<DirectedRoad>
    {
        public OsmMapMatcher(RoadGraph graph)
        {
            Parameters = MapMatcherParameters.Default;
            SearchGrid = SearchGridFactory.ComputeSearchGrid(graph, DefaultValues.Nearby_Road_Radius_In_Meters);
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
            result.Parameters = Parameters;
            return result;
        }
    }
}