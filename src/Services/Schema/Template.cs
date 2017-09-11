using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Fileharbor.Services.Schema
{
    public class Template
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("include_columns")]
        public IEnumerable<string> ColumnIncludes { get; set; }

        [JsonProperty("include_contenttypes")]
        public IEnumerable<string> ContentTypeIncludes { get; set; }

        [JsonIgnore]
        public IEnumerable<Column> Columns { get; set; }

        [JsonIgnore]
        public IEnumerable<ContentType> ContentTypes { get; set; }

        [JsonIgnore]
        public CultureInfo Language { get; set; }
    }
}
