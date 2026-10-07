using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Controllers.System;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IMailService _mailService;

        public SystemController(IMailService mailService)
        {
            _mailService = mailService;
        }

        /// <summary>
        /// Send email to the aarhus vandsportscenter admin
        /// </summary>
        [HttpPost("sendContactEmail", Name = nameof(SendContactEmail))]
        public async Task<IActionResult> SendContactEmail([FromBody] SendContactEmailRequest body)
        {
            await _mailService.SendContactEmail(body.FromEmail, body.FullName, body.Comment);
            return NoContent();
        }
    }
}
