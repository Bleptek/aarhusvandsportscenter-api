using System;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalStatisticsItemResponse
    {
        public RentalStatisticsItemResponse(int productId, string productName, string productNamePlural, int count)
        {
            ProductId = productId;
            ProductName = productName;
            ProductNamePlural = productNamePlural;
            Count = count;
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNamePlural { get; set; }
        public int Count { get; set; }
    }
}