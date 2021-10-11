using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class ContentPageImageEntity : BaseEntity
    {
        public ContentPageImageEntity()
        {
        }

        public ContentPageImageEntity(string path)
        {
            Path = path;
        }

        [Required]
        public string Path { get; set; }

        [Required]
        public int ContentPageId { get; set; }

        public ContentPageEntity ContentPage { get; set; }
    }
}
