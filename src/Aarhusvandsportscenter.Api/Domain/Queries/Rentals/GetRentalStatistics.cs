using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.Rentals
{
    public static class GetRentalStatistics
    {
        // query
        public record Query() : IRequest<IEnumerable<RentalItemEntity>>;

        // handler
        public class Handler : IRequestHandler<Query, IEnumerable<RentalItemEntity>>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<IEnumerable<RentalItemEntity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var items = await _dbContext.RentalItems
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .ToListAsync();

                return items;
            }
        }
    }
}