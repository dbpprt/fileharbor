using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("contenttypes")]
    public class ContentTypeEntity
    {
        [Key]
        [ColumnName("id")]
        public Guid Id { get; set; }

        [ColumnName("collection_id")]
        public Guid CollectionId { get; set; }

        [ColumnName("parent_id")]
        public Guid? ParentId { get; set; }

        [Required]
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }

        [Required]
        [ColumnName("group_name")]
        public string GroupName { get; set; }

        [Required]
        [ColumnName("sealed")]
        public bool Sealed { get; set; }
    }
}