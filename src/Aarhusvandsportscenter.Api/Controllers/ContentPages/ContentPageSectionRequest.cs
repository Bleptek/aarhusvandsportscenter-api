using System.ComponentModel.DataAnnotations;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class ContentPageSectionRequest : ContentPageSectionBaseRequest
    {

        /// <summary>
        /// The unique identifier of the section for the content page.
        /// </summary>
        [Required]
        public string Key { get; set; }

        public ContentPageSectionEntity MapToEntity() => base.MapToEntity(Key);
    }
}