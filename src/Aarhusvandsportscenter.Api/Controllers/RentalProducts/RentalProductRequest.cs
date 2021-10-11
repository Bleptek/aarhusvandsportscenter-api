using System.ComponentModel.DataAnnotations;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProducts
{
    public class RentalProductRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string NamePlural { get; set; }
        [Required]
        public int AmountInStock { get; set; }

        /// <summary>
        /// Use this during creation, not updating
        /// </summary>
        public RentalProductEntity MapToEntity()
        {
            var result = new RentalProductEntity
            {
                AmountInStock = AmountInStock,
                Name = Name,
                NamePlural = NamePlural
            };

            return result;
        }
    }
}