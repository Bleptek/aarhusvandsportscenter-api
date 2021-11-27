using System;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalStatisticsResponse
    {
        public RentalStatisticsResponse(){}
        public RentalStatisticsResponse(IEnumerable<RentalItemEntity> items)
        {
            Items = items
                .GroupBy(x => x.ProductId)
                .Select(x => new RentalStatisticsItemResponse(
                    x.Key,
                    x.First().Product.Name,
                    x.First().Product.NamePlural,
                    x.Sum(q => q.Count)));
        }

        public IEnumerable<RentalStatisticsItemResponse> Items { get; set; }
    }
}