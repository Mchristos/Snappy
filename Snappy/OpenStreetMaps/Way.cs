using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.OpenStreetMaps
{
    [DisplayName("way")]
    public class Way : Element
    {
        [JsonProperty("nodes")]
        public long[] Nodes { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }
    }
}
