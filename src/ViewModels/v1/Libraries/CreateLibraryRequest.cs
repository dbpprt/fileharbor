using System.ComponentModel.DataAnnotations;

namespace Fileharbor.ViewModels.v1.Libraries
{
    public class CreateLibraryRequest
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}