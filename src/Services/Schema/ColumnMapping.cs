using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fileharbor.Services.Schema
{
    public class ColumnMapping
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("default")]
        public string Default { get; set; }
    }
}
