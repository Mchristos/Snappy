using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.OpenStreetMaps
{
    [DisplayName("node")]
    public class OsmNode : Element, IEquatable<OsmNode>
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        public bool Equals(OsmNode other)
        {
            return Id == other.Id;
        }
    }
}
