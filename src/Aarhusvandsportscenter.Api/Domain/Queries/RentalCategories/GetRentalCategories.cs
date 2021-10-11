using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.RentalCategories
{
    public static class GetRentalCategories
    {
        // query
        public record Query() : IRequest<IEnumerable<RentalCategoryEntity>>;

        // handler
        public class Handler : IRequestHandler<Query, IEnumerable<RentalCategoryEntity>>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<IEnumerable<RentalCategoryEntity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rentals = await _dbContext.RentalCategories
                    .AsNoTracking()
                    .ToArrayAsync();

                return rentals;
            }
        }
    }
}