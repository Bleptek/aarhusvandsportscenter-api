using System;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentaltemCompactResponse
    {
        public RentaltemCompactResponse() { }
        public RentaltemCompactResponse(RentalItemEntity model)
        {
            Count = model.Count;
            ProductId = model.ProductId;
            ProductName = model.Product.Name;
            ProductNamePlural = model.Product.NamePlural;
        }

        public int Count { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNamePlural { get; set; }
    }
}