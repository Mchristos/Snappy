using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Snappy.DataStructures;
using Snappy.Functions;
using Snappy.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Snappy.OpenStreetMaps
{
    public class OsmHelpers
    {
        public static string GetOsmResponse(string apiUrl, BoundingBox boundingBox, bool highwayTags = true, bool railTags = true)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(apiUrl)
            };
            string left = boundingBox.LngLowerBound.ToString();
            string bottom = boundingBox.LatLowerBound.ToString();
            string right = boundingBox.LngUpperBound.ToString();
            string top = boundingBox.LatUpperBound.ToString();

            string wayQueryString = "";
            if (!highwayTags && !railTags) { throw new ArgumentException("Either highway or rail tags must be true"); }
            if (highwayTags)
            {
                wayQueryString += @"way[highway = motorway]({0},{1},{2},{3});
                                                way[highway = trunk]({0},{1},{2},{3});
                                                way[highway = primary]({0},{1},{2},{3});
                                                way[highway = secondary]({0},{1},{2},{3});
                                                way[highway = tertiary]({0},{1},{2},{3});
                                                way[highway = unclassified]({0},{1},{2},{3});
                                                way[highway = residential]({0},{1},{2},{3});
                                                way[highway = service]({0},{1},{2},{3});
                                                way[highway = motorway_link]({0},{1},{2},{3});
                                                way[highway = trunk_link]({0},{1},{2},{3});
                                                way[highway = primary_link]({0},{1},{2},{3});
                                                way[highway = secondary_link]({0},{1},{2},{3});
                                                way[highway = tertiary_link]({0},{1},{2},{3});
                                                way[highway = living_street]({0},{1},{2},{3});
                                                way[highway = bus_guideway	]({0},{1},{2},{3});
                                                way[highway = road]({0},{1},{2},{3});
                                                way[highway = track]({0},{1},{2},{3});";
            }
            if (railTags)
            {
                wayQueryString += "way[railway]({0},{1},{2},{3});";
            }
            string queryString = String.Format(@"/interpreter?data=[out:json];(" + wayQueryString + "); (._;>;);out;", bottom, left, top, right);
            string url = apiUrl + queryString;

            Console.WriteLine("Calling Overpass API");
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.SendAsync(httpRequest).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The following went wrong with API call: " + response.ReasonPhrase);
            }
            string result = response.Content.ReadAsStringAsync().Result;
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString() + " seconds to get response from " + apiUrl);

            return result;
        }

        public static List<Element> GetOsmElements(string osmResponse)
        {
            var replaceRegex = new Regex("\"type\"");
            var replacedString = replaceRegex.Replace(osmResponse, "\"$type\"");
            ElementsCollection elementsCollection = JsonConvert.DeserializeObject<ElementsCollection>(replacedString, new JsonSerializerSettings
            {
                Binder = new DisplayNameSerializationBinder(),
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return elementsCollection.Elements;
        }

 
    }








}