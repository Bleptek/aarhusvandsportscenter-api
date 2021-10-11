using System;
using System.Collections.Generic;
using LHSBrackets.ModelBinder;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalFilterRequest : FilterRequest
    {
        public FilterOperations<int> CategoryId { get; set; } = new FilterOperations<int>();
        public FilterOperations<DateTime> StartDate { get; set; } = new FilterOperations<DateTime>();
        public FilterOperations<DateTime> EndDate { get; set; } = new FilterOperations<DateTime>();



        public override IEnumerable<(string PropertyName, Action<string> BindValue)> GetBinders()
        {
            var binders = new List<(string PropertyName, Action<string> BindValue)>();

            binders.AddRange(BuildFilterOperationBinders(CategoryId, nameof(CategoryId)));
            binders.AddRange(BuildFilterOperationBinders(StartDate, nameof(StartDate)));
            binders.AddRange(BuildFilterOperationBinders(EndDate, nameof(EndDate)));

            return binders;
        }
    }
}