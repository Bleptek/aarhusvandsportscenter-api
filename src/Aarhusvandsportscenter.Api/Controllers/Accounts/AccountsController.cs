using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Aarhusvandsportscenter.Api.Domain.Commands.Accounts;
using Microsoft.AspNetCore.Authorization;
using Aarhusvandsportscenter.Api.Infastructure.Authorization;

namespace Aarhusvandsportscenter.Api.Controllers.Accounts
{

    /// <summary>
    /// Accounts API
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJwtTokenHelper _jwtTokenHelper;

        public AccountsController(
            IMediator mediator,
            IJwtTokenHelper jwtTokenHelper)
        {
            _mediator = mediator;
            _jwtTokenHelper = jwtTokenHelper;
        }

        /// <summary>
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPost(Name = nameof(CreateAccount))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest body)
        {
            var account = await _mediator.Send(new CreateAccount.Command(body.FullName, body.Email));
            return Created("", new AccountResponse(account));
        }

        [HttpPost("login", Name = nameof(LoginAccount))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<LoginResponse>> LoginAccount([FromBody] LoginAccountRequest body)
        {
            var account = await _mediator.Send(new LoginAccount.Command(body.Email, body.Password));
            var jwtToken = _jwtTokenHelper.GenerateJwtToken(account);
            return Ok(new LoginResponse(jwtToken, account));
        }

        [HttpPost("resetPassword", Name = nameof(ResetAccountPassword))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ResetAccountPassword([FromBody] ResetAccountPasswordRequest body)
        {
            await _mediator.Send(new ResetAccountPassword.Command(body.Email));
            return NoContent();
        }

        [HttpPut("updatePassword", Name = nameof(UpdateAccountPassword))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateAccountPassword([FromBody] UpdateAccountPasswordRequest body)
        {
            await _mediator.Send(new UpdateAccountPassword.Command(body.ResetPasswordToken, body.Password));
            return NoContent();
        }
    }
}