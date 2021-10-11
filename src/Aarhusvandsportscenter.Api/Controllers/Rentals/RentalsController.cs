using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Domain.Commands.Rentals;
using Aarhusvandsportscenter.Api.Domain.Queries.Rentals;
using Aarhusvandsportscenter.Api.Domain.Queries.Rentals.GetRentalsByFilter;
using LHSBrackets.ModelBinder;
using Microsoft.AspNetCore.Authorization;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{

    /// <summary>
    /// Rentals API
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RentalsController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new rental.
        /// A confirmation email will be sent to the customer
        /// </summary>
        [HttpPost(Name = nameof(CreateRental))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<RentalCompactResponse>> CreateRental([FromBody] CreateRentalRequest body)
        {
            var rental = await _mediator.Send(new CreateRental.Command(body.MapToEntity()));
            return Created("", new RentalCompactResponse(rental));
        }

        /// <summary>
        /// Update rental
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{id}", Name = nameof(UpdateRental))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateRental(
            [FromRoute] int id,
            [FromBody] UpdateRentalRequest body)
        {
            await _mediator.Send(new UpdateRental.Command(id, body.CategoryId, body.Comment));
            return NoContent();
        }

        /// <summary>
        /// Finish rental.
        /// This will mark a rental as finished, effectively enabling the rented equipment for other rentals.
        /// </summary>
        [HttpPut("{id}/finish", Name = nameof(FinishRental))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> FinishRental(
            [FromRoute] int id,
            [FromQuery] string phone)
        {
            await _mediator.Send(new FinishRental.Command(id, phone));
            return NoContent();
        }

        /// <summary>
        /// Delete rental.
        /// Phone is required to match. This is used to verify that you are allowed to delete the rental.
        /// </summary>
        [HttpDelete("{id}", Name = nameof(DeleteRental))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteRental(
            [FromRoute] int id,
            [FromQuery] string phone)
        {
            await _mediator.Send(new DeleteRental.Command(id, phone));
            return NoContent();
        }

        /// <summary>
        /// Get all rental rentals by date
        /// </summary>
        [HttpGet(Name = nameof(GetRentals))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<RentalCompactResponse>>> GetRentals(
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate

        )
        {
            var rentals = await _mediator.Send(new GetRentalsByDateRange.Query(startDate, endDate));
            return Ok(rentals.Select(x => new RentalCompactResponse(x)));
        }

        /// <summary>
        /// Get rental
        /// </summary>
        [HttpGet("{id}", Name = nameof(GetRental))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<RentalCompactResponse>> GetRental([FromRoute] int id)
        {
            var rental = await _mediator.Send(new GetRental.Query(id));
            return Ok(new RentalCompactResponse(rental));
        }

        /// <summary>
        /// Get all rental rentals detailed
        /// Requires authorization
        /// </summary>
        [HttpGet("detailed", Name = nameof(GetRentalsDetailed))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize]
        public async Task<IActionResult> GetRentalsDetailed(
            [FromQuery][ModelBinder(typeof(FilterModelBinder))] RentalFilterRequest filters
        )
        {
            var rentals = await _mediator.Send(new GetRentalsByFilter.Query(filters));
            return Ok(rentals.Select(x => new RentalResponse(x)));
        }
    }
}