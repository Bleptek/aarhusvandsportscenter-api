using System;
using LHSBrackets.ModelBinder;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalFilterRequest : FilterRequest
    {
        public FilterOperations<int> CategoryId { get; set; } = new FilterOperations<int>();
        public FilterOperations<DateTime> StartDate { get; set; } = new FilterOperations<DateTime>();
        public FilterOperations<DateTime> EndDate { get; set; } = new FilterOperations<DateTime>();
    }
}