using System.ComponentModel.DataAnnotations;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class CreateRentalItemRequest
    {
        [Required]
        public int Count { get; set; }

        [Required]
        public int ProductId { get; set; }


        public RentalItemEntity MapToEntity()
        {
            var result = new RentalItemEntity
            {
                Count = Count,
                ProductId = ProductId
            };

            return result;
        }
    }
}