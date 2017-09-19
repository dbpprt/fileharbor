using System.ComponentModel.DataAnnotations;

namespace Fileharbor.ViewModels.v1.Collections
{
    public class CreateCollectionRequest
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsDefault { get; set; }
    }
}