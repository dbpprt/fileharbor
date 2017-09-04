using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Entities
{
    [Table("Users")]
    public class UserEntity
    {
        [Key, ColumnName("id")]
        public Guid Id { get; set; }

        [Required, EmailAddress, ColumnName("email")]
        public string MailAddress { get; set; }

        [Required, ColumnName("password_hash")]
        public string PasswordHash { get; set; }

        [Required, ColumnName("givenname")]
        public string GivenName { get; set; }

        [Required, ColumnName("surname")]
        public string SurName { get; set; }

        [Required, ColumnName("validated")]
        public bool Validated { get; set; }

        [Required, ColumnName("last_login")]
        public DateTime? LastLogin { get; set; }
    }
}
