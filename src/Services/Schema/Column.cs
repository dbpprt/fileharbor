using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fileharbor.Services.Schema
{
    public class Column
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("group")]
        public string Goup { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("settings")]
        public JObject Settings { get; set; }

        [JsonProperty("sealed")]
        public bool Sealed { get; set; }
    }
}
