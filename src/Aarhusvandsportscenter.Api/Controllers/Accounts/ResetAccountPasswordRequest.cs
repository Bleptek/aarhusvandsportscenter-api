using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers.Accounts
{
    public class ResetAccountPasswordRequest
    {
        [Required]
        public string Email { get; set; }
    }
}