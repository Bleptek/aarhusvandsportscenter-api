using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers.System
{
    public class SendContactEmailRequest
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string FromEmail { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}