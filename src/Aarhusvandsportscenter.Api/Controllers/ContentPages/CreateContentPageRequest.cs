using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class CreateContentPageRequest
    {
        /// <summary>
        /// Optional
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The unique identifier of the contentPage
        /// </summary>
        [Required]
        public string Key { get; set; }

        /// <summary>
        /// Optional. Sections can be added later with the update endpoint.
        /// </summary>
        public IEnumerable<ContentPageSectionRequest> Sections { get; set; } = new ContentPageSectionRequest[0];

        public ContentPageEntity MapToEntity()
        {
            return new ContentPageEntity
            {
                Title = Title,
                Key = Key,
                Sections = Sections.Select(x => x.MapToEntity()).ToArray()
            };
        }
    }
}