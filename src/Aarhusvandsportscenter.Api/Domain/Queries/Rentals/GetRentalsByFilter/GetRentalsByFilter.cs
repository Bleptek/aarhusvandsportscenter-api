using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Controllers.Rentals;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using LHSBrackets.ModelBinder.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.Rentals.GetRentalsByFilter
{
    public static class GetRentalsByFilter
    {
        // query
        public record Query(RentalFilterRequest Filter) : IRequest<IEnumerable<RentalEntity>>;

        // handler
        public class Handler : IRequestHandler<Query, IEnumerable<RentalEntity>>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<IEnumerable<RentalEntity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rentals = await _dbContext.Rentals
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Items).ThenInclude(i => i.Product)
                    .ApplyFilters(x => x.CategoryId, request.Filter.CategoryId)
                    .ApplyFilters(x => x.StartDate, request.Filter.StartDate)
                    .ApplyFilters(x => x.EndDate, request.Filter.EndDate)
                    .ToListAsync();

                return rentals;
            }
        }
    }
}