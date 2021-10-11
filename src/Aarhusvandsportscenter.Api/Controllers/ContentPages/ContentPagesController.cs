using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using Aarhusvandsportscenter.Api.Domain.Queries.ContentPages;
using Aarhusvandsportscenter.Api.Domain.Commands.ContentPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{

    /// <summary>
    /// ContentPage API
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class ContentPagesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentPagesController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }


        #region ContentPages
        /// <summary>
        /// Get specific content page
        /// </summary>
        [HttpGet("{pageKey}", Name = nameof(GetContentPage))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ContentPageResponse>> GetContentPage([FromRoute] string pageKey)
        {
            var contentPage = await _mediator.Send(new GetContentPageByKey.Query(pageKey));
            return Ok(new ContentPageResponse(contentPage, _httpContextAccessor.HttpContext.Request));
        }

        /// <summary>
        /// Get all content pages
        /// </summary>
        [HttpGet(Name = nameof(GetContentPages))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ContentPageResponse>>> GetContentPages()
        {
            var contentPages = await _mediator.Send(new GetContentPages.Query());
            return Ok(contentPages.Select(x => new ContentPageResponse(x)));
        }

        /// <summary>
        /// Create a content page
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPost(Name = nameof(CreateContentPage))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<ContentPageResponse>> CreateContentPage([FromBody] CreateContentPageRequest body)
        {
            var contentPage = await _mediator.Send(new CreateContentPage.Command(body.MapToEntity()));
            return Created("", new ContentPageResponse(contentPage));
        }


        /// <summary>
        /// Update an specific content page
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{pageKey}", Name = nameof(UpdateContentPage))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ContentPageResponse>> UpdateContentPage(
            [FromRoute] string pageKey,
            [FromBody] UpdateContentPageRequest body)
        {
            var contentPage = await _mediator.Send(new UpdateContentPage.Command(
                pageKey,
                body.Title,
                body.Sections.Select(x => x.MapToEntity())));
            return Ok(new ContentPageResponse(contentPage));
        }
        #endregion

        #region ContentPageImages
        /// <summary>
        /// Upload a content page image
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPost("{pageKey}/uploadImage", Name = nameof(UploadContentPageImage))]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ContentPageImageResponse>> UploadContentPageImage(
            [FromRoute] string pageKey,
            IFormFile file)
        {
            var image = await _mediator.Send(new UploadContentPageImage.Command(pageKey, file));
            return Created("", new ContentPageImageResponse(image, _httpContextAccessor.HttpContext.Request));
        }
        #endregion

        #region ContentPageSection
        /// <summary>
        /// Get a specific content page section
        /// </summary>
        [HttpGet("{pageKey}/sections/{sectionKey}", Name = nameof(GetContentPageSection))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ContentPageSectionResponse>> GetContentPageSection(
            [FromRoute] string pageKey,
            [FromRoute] string sectionKey)
        {
            var contentPage = await _mediator.Send(new GetContentPageSection.Query(pageKey, sectionKey));
            return Ok(new ContentPageSectionResponse(contentPage));
        }

        /// <summary>
        /// Update a specific content page section
        /// Requires authorization
        /// </summary>
        [Authorize]
        [HttpPut("{pageKey}/sections/{sectionKey}", Name = nameof(UpdateContentPageSection))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ContentPageSectionResponse>> UpdateContentPageSection(
            [FromRoute] string pageKey,
            [FromRoute] string sectionKey,
            [FromBody] ContentPageSectionBaseRequest body)
        {
            var contentPage = await _mediator.Send(new UpdateContentPageSection.Command(
                pageKey,
                body.MapToEntity(sectionKey)));
            return Ok(new ContentPageSectionResponse(contentPage));
        }
        #endregion
    }
}