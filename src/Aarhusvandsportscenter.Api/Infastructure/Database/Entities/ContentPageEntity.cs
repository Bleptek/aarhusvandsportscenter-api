using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class ContentPageEntity : BaseEntity
    {
        public ContentPageEntity()
        {
        }

        public ContentPageEntity(string key, string title)
        {
            Key = key;
            Title = title;
        }

        [Required]
        public string Key { get; set; }
        public string Title { get; set; }

        public IEnumerable<ContentPageSectionEntity> Sections { get; set; }
        public IEnumerable<ContentPageImageEntity> Images { get; set; }
    }
}
