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
                wayQueryString += @"way[highway~""^(motorway|trunk|primary|secondary|tertiary|unclassified|residential|service|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|living_street|bus_guideway|road|track)$""]; ";
            }
            if (railTags)
            {
                wayQueryString += "way[railway];";
            }
            string queryString = String.Format(@"/interpreter?data=[bbox][out:json];(" + wayQueryString + "); (._;>;);out;&bbox={0},{1},{2},{3}", left, bottom, right, top);
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