using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProductPrices
{

    /// <summary>
    /// Rental product prices API
    /// </summary>
    [Route("api/v1/rentalProducts/{id}/prices")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class RentalProductPricesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RentalProductPricesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update an existing rental product price
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{priceId}", Name = nameof(UpdateRentalProductPrice))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<RentalProductPriceResponse>> UpdateRentalProductPrice(
            [FromRoute] int id, // we're not actually using this. just here for REST design
            [FromRoute] int priceId,
            [FromBody] RentalProductPriceRequest body
        )
        {
            var product = await _mediator.Send(new UpdateRentalProductPrice.Command(priceId, body.Quantity, body.UnitPrice));
            return Ok(new RentalProductPriceResponse(product));
        }

        /// <summary>
        /// Create a new rental product price
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPost(Name = nameof(CreateRentalProductPrice))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<IEnumerable<RentalProductPriceResponse>>> CreateRentalProductPrice(
            [FromRoute] int id,
            [FromBody] RentalProductPriceRequest body
        )
        {
            var product = await _mediator.Send(new CreateRentalProductPrice.Command(id, body.MapToEntity(id)));
            return Created("", new RentalProductPriceResponse(product));
        }

        /// <summary>
        /// Delete rental product
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpDelete("{priceId}", Name = nameof(DeleteRentalProductPrice))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteRentalProductPrice(
            [FromRoute] int id,
            [FromRoute] int priceId)
        {
            await _mediator.Send(new DeleteRentalProductPrice.Command(priceId));
            return NoContent();
        }
    }
}