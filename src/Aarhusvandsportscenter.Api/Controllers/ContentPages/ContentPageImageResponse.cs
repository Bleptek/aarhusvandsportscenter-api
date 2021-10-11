using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Microsoft.AspNetCore.Http;

namespace Aarhusvandsportscenter.Api.Controllers.ContentPages
{
    public class ContentPageImageResponse
    {
        public ContentPageImageResponse() { }
        public ContentPageImageResponse(ContentPageImageEntity model, HttpRequest httpRequest)
        {
            Path = $"{httpRequest?.Scheme}://{httpRequest?.Host}/{model?.Path}";
            Id = model.Id;
        }

        /// <summary>
        /// Public path to the file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
    }
}