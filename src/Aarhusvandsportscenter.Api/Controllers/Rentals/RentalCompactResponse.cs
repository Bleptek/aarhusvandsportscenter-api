using System;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalCompactResponse
    {
        public RentalCompactResponse() { }
        public RentalCompactResponse(RentalEntity model)
        {
            Id = model.Id;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            Items = model.Items.Select(x => new RentaltemCompactResponse(x)).ToArray();
        }

        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public IEnumerable<RentaltemCompactResponse> Items { get; set; }
    }
}