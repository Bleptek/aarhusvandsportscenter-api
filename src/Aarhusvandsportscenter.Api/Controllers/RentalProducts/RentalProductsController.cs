using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts;
using Aarhusvandsportscenter.Api.Domain.Queries.RentalProducts;
using Microsoft.AspNetCore.Authorization;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProducts
{

    /// <summary>
    /// Rental products API
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class RentalProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RentalProductsController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all rental products
        /// </summary>
        [HttpGet(Name = nameof(GetRentalProducts))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<RentalProductResponse>>> GetRentalProducts()
        {
            var products = await _mediator.Send(new GetRentalProducts.Query());
            return Ok(products.Select(x => new RentalProductResponse(x)));
        }

        /// <summary>
        /// Update an existing rental product
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{id}", Name = nameof(UpdateRentalProduct))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<RentalProductResponse>>> UpdateRentalProduct(
            [FromRoute] int id,
            [FromBody] RentalProductRequest body
        )
        {
            var product = await _mediator.Send(new UpdateRentalProduct.Command(id, body.Name, body.NamePlural, body.AmountInStock));
            return Ok(new RentalProductResponse(product));
        }

        /// <summary>
        /// Create a new rental product
        /// Requires authorization
        /// </summary>
        /// <remarks>
        /// A default price object with a quantity of 1 will be created for the product.
        /// </remarks>
        [Authorize]
        [HttpPost(Name = nameof(CreateRentalProduct))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<IEnumerable<RentalProductResponse>>> CreateRentalProduct(
            [FromBody] RentalProductRequest body
        )
        {
            var product = await _mediator.Send(new CreateRentalProduct.Command(body.MapToEntity()));
            return Created("", new RentalProductResponse(product));
        }

        /// <summary>
        /// Delete rental product
        /// Requires authorization
        /// </summary>
        /// <remarks>
        /// A product that is used by any existing rentals cannot be deleted.
        /// </remarks>
        [Authorize]
        [HttpDelete("{id}", Name = nameof(DeleteRentalProduct))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteRentalProduct(
            [FromRoute] int id)
        {
            await _mediator.Send(new DeleteRentalProduct.Command(id));
            return NoContent();
        }
    }
}