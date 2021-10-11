using System;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Microsoft.AspNetCore.Http;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class ContentPageResponse
    {
        public ContentPageResponse() { }
        public ContentPageResponse(ContentPageEntity model, HttpRequest httpRequest = null)
        {
            Title = model.Title;
            Key = model.Key;
            Sections = model.Sections?.Select(x => new ContentPageSectionResponse(x)) ?? new ContentPageSectionResponse[0];
            Images = model.Images?.Select(x => new ContentPageImageResponse(x, httpRequest)) ?? new ContentPageImageResponse[0];
            CreatedDate = model.CreatedDate;
            LastModifiedDate = model.LastModifiedDate;
        }

        /// <summary>
        /// Optional
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Key { get; set; }
        public IEnumerable<ContentPageSectionResponse> Sections { get; set; }
        public IEnumerable<ContentPageImageResponse> Images { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}