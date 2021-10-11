using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using System.Linq;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalCategories
{
    public static class DeleteRentalCategory
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
                var category = await _dbContext.RentalCategories
                    .Include(x => x.Rentals)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);
                if (category == null)
                    throw new NotFoundException(ErrorCodes.Rentals.CATEGORY_DOESNT_EXIST);

                if(category.Rentals.Any())
                    throw new BusinessRuleException(ErrorCodes.Rentals.CATEGORY_IS_IN_USE);

                _dbContext.RentalCategories.Remove(category);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}