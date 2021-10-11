using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Domain.Exceptions;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Rentals
{
    public static class UpdateRental
    {
        public record Command(int Id, int CategoryId, string Comment) : IRequest<RentalEntity>;

        public class Handler : IRequestHandler<Command, RentalEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(
                AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RentalEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingRental = await _dbContext.Rentals.FirstOrDefaultAsync(x => x.Id == request.Id);
                var existingCategory = await _dbContext.RentalCategories.FirstOrDefaultAsync(x => x.Id == request.CategoryId);

                if (existingRental == null)
                    throw new NotFoundException(ErrorCodes.Rentals.RENTAL_DOESNT_EXIST);

                if (existingRental == null)
                    throw new NotFoundException(ErrorCodes.Rentals.CATEGORY_DOESNT_EXIST);

                existingRental.CategoryId = request.CategoryId;
                existingRental.AdminComment = request.Comment;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return existingRental;
            }
        }
    }
}