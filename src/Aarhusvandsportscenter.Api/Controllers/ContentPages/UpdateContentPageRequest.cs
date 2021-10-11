using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class UpdateContentPageRequest
    {
        /// <summary>
        /// Optional
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// This will overwrite existing sections. If left empty/null, all existing sections will be removed
        /// </summary>
        public IEnumerable<ContentPageSectionRequest> Sections { get; set; } = new ContentPageSectionRequest[0];
    }
}