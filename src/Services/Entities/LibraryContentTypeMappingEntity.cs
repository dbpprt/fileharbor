using System;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("library_contenttype_mappings")]
    public class LibraryContentTypeMappingEntity
    {
        [ColumnName("library_id")]
        public Guid ColumnId { get; set; }

        [ColumnName("contenttype_id")]
        public Guid ContentTypeId { get; set; }

        [ColumnName("collection_id")]
        public Guid CollectionId { get; set; }

        [ColumnName("visible")]
        public bool Visible { get; set; }
    }
}