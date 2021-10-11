using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class RentalProductPriceEntity : BaseEntity
    {
        public RentalProductPriceEntity(int quantity, decimal unitPrice)
        {
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public RentalProductPriceEntity()
        {
        }


        /// <summary>
        /// The required quantity to achieve the unit price
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public RentalProductEntity Product { get; set; }
    }
}