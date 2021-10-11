using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.RentalProducts
{
    public static class GetRentalProducts
    {
        // query
        public record Query() : IRequest<IEnumerable<RentalProductEntity>>;

        // handler
        public class Handler : IRequestHandler<Query, IEnumerable<RentalProductEntity>>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<IEnumerable<RentalProductEntity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rentals = await _dbContext.RentalProducts
                    .AsNoTracking()
                    .Include(x => x.Prices)
                    .ToArrayAsync();

                return rentals;
            }
        }
    }
}