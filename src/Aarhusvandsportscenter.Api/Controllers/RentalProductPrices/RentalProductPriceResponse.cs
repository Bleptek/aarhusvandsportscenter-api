using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalProductPrices
{
    public class RentalProductPriceResponse
    {
        public RentalProductPriceResponse() { }
        public RentalProductPriceResponse(RentalProductPriceEntity model)
        {
            Id = model.Id;
            ProductId = model.ProductId;
            Quantity = model.Quantity;
            UnitPrice = model.UnitPrice;
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}