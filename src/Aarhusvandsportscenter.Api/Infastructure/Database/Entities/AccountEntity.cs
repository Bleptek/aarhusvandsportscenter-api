using System;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class AccountEntity : BaseEntity
    {
        public AccountEntity(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
        }

        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
        public Guid? ResetPasswordToken { get; set; }
    }
}
