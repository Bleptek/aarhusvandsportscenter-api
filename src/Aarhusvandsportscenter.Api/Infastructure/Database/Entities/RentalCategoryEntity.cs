using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class RentalCategoryEntity : BaseEntity
    {
        public RentalCategoryEntity()
        {
        }

        public RentalCategoryEntity(string name, string colorCode, bool isDefault = false)
        {
            Name = name;
            ColorCode = colorCode;
            IsDefault = isDefault;
        }

        [Required]
        public string Name { get; set; }
        public string ColorCode { get; set; }
        [Required]
        public bool IsDefault { get; set; } = false;

        public IEnumerable<RentalEntity> Rentals { get; set; }
    }
}
