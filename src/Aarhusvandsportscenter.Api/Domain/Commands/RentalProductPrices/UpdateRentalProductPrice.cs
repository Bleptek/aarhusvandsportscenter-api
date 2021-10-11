using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Domain.Exceptions;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts
{
    public static class UpdateRentalProductPrice
    {
        public record Command(int Id, int Quantity, decimal UnitPrice) : IRequest<RentalProductPriceEntity>;

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
                var existingEntry = await _dbContext.RentalProductPrices.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (existingEntry == null)
                    throw new NotFoundException(ErrorCodes.Rentals.PRICE_DOESNT_EXIST);

                if(existingEntry.Quantity == 1 && request.Quantity != 1)
                    throw new BusinessRuleException(ErrorCodes.Rentals.PRICE_QUANTITY_1_MUST_EXIST);

                existingEntry.Quantity = request.Quantity;
                existingEntry.UnitPrice = request.UnitPrice;

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

                return existingEntry;
            }
        }
    }
}