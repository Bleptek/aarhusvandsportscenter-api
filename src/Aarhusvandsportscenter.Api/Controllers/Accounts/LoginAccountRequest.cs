using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers.Accounts
{
    public class LoginAccountRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}