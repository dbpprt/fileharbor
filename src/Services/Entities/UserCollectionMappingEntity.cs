using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("user_collection_mappings")]
    public class UserCollectionMappingEntity
    {
        [Key]
        [ColumnName("user_id")]
        public Guid UserId { get; set; }

        [ColumnName("collection_id")]
        public Guid CollectionId { get; set; }

        [ColumnName("is_default")]
        public bool IsDefault { get; set; }
    }
}