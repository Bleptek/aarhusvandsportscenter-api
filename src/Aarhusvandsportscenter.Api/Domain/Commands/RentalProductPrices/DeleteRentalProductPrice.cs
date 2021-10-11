using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts
{
    public static class DeleteRentalProductPrice
    {
        public record Command(int Id) : IRequest;

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly AppDbContext _dbContext;

            public Handler(
                AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var entryToDelete = await _dbContext.RentalProductPrices.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (entryToDelete == null)
                    throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                if(entryToDelete.Quantity == 1)
                    throw new BusinessRuleException(ErrorCodes.Rentals.PRICE_QUANTITY_1_MUST_EXIST);

                _dbContext.RentalProductPrices.Attach(entryToDelete);
                _dbContext.RentalProductPrices.Remove(entryToDelete);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}