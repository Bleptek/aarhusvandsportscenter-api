using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using System.Linq;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalProducts
{
    public static class DeleteRentalProduct
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
                var rentalProduct = await _dbContext.RentalProducts
                    .Include(x => x.RentalItems)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);
                if (rentalProduct == null)
                    throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                if(rentalProduct.RentalItems.Any())
                    throw new BusinessRuleException(ErrorCodes.Rentals.PRODUCT_IS_IN_USE);

                _dbContext.RentalProducts.Attach(rentalProduct);
                _dbContext.RentalProducts.Remove(rentalProduct);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}