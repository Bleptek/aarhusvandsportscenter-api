using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using System.Linq;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts
{
    public static class CreateRentalProductPrice
    {
        public record Command(int ProductId, RentalProductPriceEntity RentalProductPrice) : IRequest<RentalProductPriceEntity>;

        public class Handler : IRequestHandler<Command, RentalProductPriceEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(
                AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RentalProductPriceEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingProduct = await _dbContext.RentalProducts
                    .Include(x => x.Prices)
                    .FirstOrDefaultAsync(x => x.Id == request.ProductId);
                if (existingProduct == null)
                    throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                _dbContext.RentalProductPrices.Add(request.RentalProductPrice);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is MySqlException mysqlEx &&
                        mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                        throw new ConflictException(ErrorCodes.Rentals.PRICE_CONFLICT);

                    throw;
                }

                return request.RentalProductPrice;
            }
        }
    }
}