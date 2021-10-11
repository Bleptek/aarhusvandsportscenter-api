using System.ComponentModel.DataAnnotations;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalCategories
{

    public class RentalCategoryRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ColorCode { get; set; }

        [Required]
        public bool IsDefault { get; set; } = false;
    }
}