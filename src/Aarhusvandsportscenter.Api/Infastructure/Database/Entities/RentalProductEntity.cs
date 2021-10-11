using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class RentalProductEntity : BaseEntity
    {

        public RentalProductEntity()
        {

        }

        public RentalProductEntity(string name, string namePlural, int amountInStock)
        {
            Name = name;
            NamePlural = namePlural;
            AmountInStock = amountInStock;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public string NamePlural { get; set; }
        
        [Required]
        public int AmountInStock { get; set; }

        public IEnumerable<RentalItemEntity> RentalItems { get; set; }
        public IEnumerable<RentalProductPriceEntity> Prices { get; set; }

        public void MapFrom(RentalProductEntity mapFrom)
        {
            Name = mapFrom.Name;
            AmountInStock = mapFrom.AmountInStock;
        }
    }
}