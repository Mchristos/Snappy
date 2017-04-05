using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Snappy.OpenStreetMaps
{
    [DisplayName("way")]
    public class Way : Element , IEquatable<Way>
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("nodes")]
        public long[] Nodes { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        public bool Equals(Way other)
        {
            return Id == other.Id;
        }
    }
}