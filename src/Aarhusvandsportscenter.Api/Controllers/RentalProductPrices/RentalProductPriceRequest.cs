using System.ComponentModel.DataAnnotations;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProductPrices
{
    /// <summary>
    /// Our pricing model follows the volume pricing principle.
    /// The more units you rent and the longer the period, the less price for each unit.
    /// </summary>
    public class RentalProductPriceRequest
    {
        [Required]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// This is the quantity required to achieve the specified unit price.
        /// Quantity considers how many of the same product are rented but also for how many days.
        /// For instance: renting two units for three days results in a quantity of 6 (2x3). <br></br>
        /// 
        /// No two price objects with the same quantity can exist for a product.
        /// </summary>
        [Required]
        public int Quantity { get; set; }


        public RentalProductPriceEntity MapToEntity(int productId)
        {
            var result = new RentalProductPriceEntity
            {
                UnitPrice = UnitPrice,
                Quantity = Quantity,
                ProductId = productId
            };

            return result;
        }
    }
}