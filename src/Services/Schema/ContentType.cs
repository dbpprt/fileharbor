using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fileharbor.Services.Schema
{
    public class ContentType
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("parent_id")]
        public Guid? ParentId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("group_name")]
        public string GroupName { get; set; }

        [JsonProperty("sealed")]
        public bool Sealed { get; set; }

        [JsonProperty("columns")]
        public IEnumerable<ColumnMapping> Columns { get; set; }
    }
}