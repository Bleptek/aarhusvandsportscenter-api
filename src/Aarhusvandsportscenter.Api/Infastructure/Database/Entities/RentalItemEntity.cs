using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class RentalItemEntity : BaseEntity
    {
        [Required]
        public int Count { get; set; }
        [Required]
        public int RentalId { get; set; }
        [Required]
        public int ProductId { get; set; }


        [ForeignKey(nameof(ProductId))]
        public RentalProductEntity Product { get; set; }

        [ForeignKey(nameof(RentalId))]
        public RentalEntity Rental { get; set; }
    }
}
