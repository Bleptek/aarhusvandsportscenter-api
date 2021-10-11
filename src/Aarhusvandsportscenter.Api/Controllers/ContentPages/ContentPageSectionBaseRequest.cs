using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class ContentPageSectionBaseRequest
    {
        /// <summary>
        /// Optional
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public string Content { get; set; }

        public ContentPageSectionEntity MapToEntity(string sectionKey)
        {
            return new ContentPageSectionEntity
            {
                Title = Title,
                Content = Content,
                Key = sectionKey
            };
        }
    }
}