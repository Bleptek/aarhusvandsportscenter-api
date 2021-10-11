using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Controllers.RentalProductPrices;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProducts
{
    public class RentalProductResponse
    {
        public RentalProductResponse() { }
        public RentalProductResponse(RentalProductEntity model)
        {
            Id = model.Id;
            Name = model.Name;
            NamePlural = model.NamePlural;
            AmountInStock = model.AmountInStock;
            Prices = model.Prices?.Select(x => new RentalProductPriceResponse(x)) ?? new RentalProductPriceResponse[0];
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
        public int AmountInStock { get; set; }

        public IEnumerable<RentalProductPriceResponse> Prices { get; set; }
    }
}