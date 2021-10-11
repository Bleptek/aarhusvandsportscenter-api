using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.Rentals
{
    public static class GetRental
    {
        // query
        public record Query(int Id) : IRequest<RentalEntity>;

        // handler
        public class Handler : IRequestHandler<Query, RentalEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RentalEntity> Handle(Query request, CancellationToken cancellationToken)
            {
                var rental = await _dbContext.Rentals
                    .AsNoTracking()
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Product)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                return rental;
            }
        }
    }
}