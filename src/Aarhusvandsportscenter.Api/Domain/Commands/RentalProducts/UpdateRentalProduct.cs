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
    public static class UpdateRentalProduct
    {
        public record Command(int Id, string Name, string NamePlural, int AmountInStock) : IRequest<RentalProductEntity>;

        public class Handler : IRequestHandler<Command, RentalProductEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(
                AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RentalProductEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingEntry = await _dbContext.RentalProducts.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (existingEntry == null)
                    throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                existingEntry.Name = request.Name;
                existingEntry.NamePlural = request.NamePlural;
                existingEntry.AmountInStock = request.AmountInStock;

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is MySqlException mysqlEx &&
                        mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                        throw new ConflictException(ErrorCodes.Rentals.PRODUCT_CONFLICT);

                    throw;
                }

                return existingEntry;
            }
        }
    }
}