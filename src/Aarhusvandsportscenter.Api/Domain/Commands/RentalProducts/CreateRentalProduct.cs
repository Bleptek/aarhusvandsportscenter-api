using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using System.Collections.Generic;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts
{
    public static class CreateRentalProduct
    {
        public record Command(RentalProductEntity RentalProduct) : IRequest<RentalProductEntity>;

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
                var product = request.RentalProduct;
                product.Prices = new List<RentalProductPriceEntity>(){
                    new RentalProductPriceEntity(1, 100)
                };
                
                await _dbContext.RentalProducts.AddAsync(request.RentalProduct);
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

                return request.RentalProduct;
            }
        }
    }
}