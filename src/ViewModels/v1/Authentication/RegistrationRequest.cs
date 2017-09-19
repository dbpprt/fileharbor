using System.ComponentModel.DataAnnotations;
using System;

namespace Fileharbor.ViewModels.v1.Authentication
{
    public class RegistrationRequest
    {
        [Required, EmailAddress]
        public string MailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string GivenName { get; set; }

        [Required]
        public string SurName { get; set; }
    }
}
