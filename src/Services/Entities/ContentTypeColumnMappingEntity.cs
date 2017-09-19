using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("contenttype_column_mappings")]
    public class ContentTypeColumnMappingEntity
    {
        [ColumnName("contenttype_id")]
        public Guid ContentTypeId { get; set; }

        [ColumnName("column_id")]
        public Guid ColumnId { get; set; }
        
        [ColumnName("collection_id")]
        public Guid CollectionId { get; set; }

        [ColumnName("required")]
        public bool required { get; set; }

        [ColumnName("visible")]
        public bool Visible { get; set; }

        [ColumnName("default_value")]
        public string DefaultValue { get; set; }
    }
}
