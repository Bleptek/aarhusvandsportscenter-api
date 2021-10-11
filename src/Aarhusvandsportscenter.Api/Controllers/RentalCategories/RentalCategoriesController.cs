using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using Aarhusvandsportscenter.Api.Domain.Queries.RentalCategories;
using System.Linq;
using Aarhusvandsportscenter.Api.Domain.Commands.RentalCategories;
using Microsoft.AspNetCore.Authorization;

namespace Aarhusvandsportscenter.Api.Controllers.RentalCategories
{

    /// <summary>
    /// Rental categories prices API
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class RentalCategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RentalCategoriesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all rental categories
        /// </summary>
        [HttpGet(Name = nameof(GetRentalCategory))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<RentalCategoryResponse>>> GetRentalCategory()
        {
            var categories = await _mediator.Send(new GetRentalCategories.Query());
            return Ok(categories.Select(x => new RentalCategoryResponse(x)));
        }

        /// <summary>
        /// Update an existing rental category
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{id}", Name = nameof(UpdateRentalCategory))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<RentalCategoryResponse>> UpdateRentalCategory(
            [FromRoute] int id,
            [FromBody] RentalCategoryRequest body
        )
        {
            var category = await _mediator.Send(new UpdateRentalCategory.Command(id, body.Name, body.ColorCode, body.IsDefault));
            return Ok(new RentalCategoryResponse(category));
        }

        /// <summary>
        /// Create a new rental category
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPost(Name = nameof(CreateRentalCategory))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<IEnumerable<RentalCategoryResponse>>> CreateRentalCategory([FromBody] RentalCategoryRequest body)
        {
            var category = await _mediator.Send(new CreateRentalCategory.Command(body.Name, body.ColorCode, body.IsDefault));
            return Created("", new RentalCategoryResponse(category));
        }

        /// <summary>
        /// Delete rental category
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpDelete("{id}", Name = nameof(DeleteRentalCategory))]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteRentalCategory([FromRoute] int id)
        {
            await _mediator.Send(new DeleteRentalCategory.Command(id));
            return NoContent();
        }
    }
}