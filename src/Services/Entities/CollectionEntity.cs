using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("collections")]
    public class CollectionEntity
    {
        [Key, ColumnName("id")]
        public Guid Id { get; set; }

        [Required, ColumnName("name")]
        public string Name { get; set; }

        [Required, ColumnName("quota")]
        public long Quota { get; set; }

        [Required, ColumnName("bytes_used")]
        public long BytesUsed { get; set; }

        [ColumnName("template_id")]
        public Guid? TemplateId { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }
    }
}
