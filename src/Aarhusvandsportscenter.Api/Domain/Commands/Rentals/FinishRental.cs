using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using System;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Rentals
{
    public static class FinishRental
    {
        public record Command(int Id, string Phone) : IRequest;

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var rentalToFinish = await _dbContext.Rentals.FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.Phone == request.Phone);

                if (rentalToFinish == null)
                    throw new NotFoundException(ErrorCodes.Rentals.RENTAL_DOESNT_EXIST);

                if (rentalToFinish.StartDate.Date > DateTime.Today)
                    throw new BusinessRuleException(ErrorCodes.Rentals.RENTAL_FINISH_ILLEGAL_BEFORE_RENTAL_START);

                rentalToFinish.Done = true;

                _dbContext.Rentals.Update(rentalToFinish);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}