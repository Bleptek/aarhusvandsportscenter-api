using System;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class ContentPageSectionResponse
    {
        public ContentPageSectionResponse() { }
        public ContentPageSectionResponse(ContentPageSectionEntity model)
        {
            Title = model.Title;
            Key = model.Key;
            Content = model.Content;
            CreatedDate = model.CreatedDate;
            LastModifiedDate = model.LastModifiedDate;
        }

        public string Title { get; set; }
        public string Key { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}