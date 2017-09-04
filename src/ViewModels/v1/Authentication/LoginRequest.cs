using System.ComponentModel.DataAnnotations;

namespace Fileharbor.ViewModels.v1.Authentication
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string MailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool Remember { get; set; }
    }
}
