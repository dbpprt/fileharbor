using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("columns")]
    public class ColumnEntity
    {
        [Key, ColumnName("id")]
        public Guid Id { get; set; }

        [ColumnName("collection_id")]
        public Guid? CollectionId { get; set; }

        [Required, ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }

        [Required, ColumnName("group")]
        public string Group { get; set; }

        [Required, ColumnName("type")]
        public string Type { get; set; }

        [Required, ColumnName("sealed")]
        public bool Sealed { get; set; }
        
        [ColumnName("settings")]
        public string Settings { get; set; }
    }
}
