using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.Rentals
{
    public static class GetRentalsByDateRange
    {
        // query
        public record Query(DateTime StartDate, DateTime EndDate) : IRequest<IEnumerable<RentalEntity>>;

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
                    .Include(x => x.Items)
                        .ThenInclude(i => i.Product)
                    .Where(x =>
                        x.StartDate.Date <= request.EndDate &&
                        x.EndDate.Date >= request.StartDate)
                    .ToArrayAsync();

                return rentals;
            }
        }
    }
}