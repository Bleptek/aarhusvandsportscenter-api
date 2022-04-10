using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Rentals
{
    public static class DeleteRental
    {
        public record Command(int Id, string Phone, bool SkipPhoneValidation) : IRequest;

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly AppDbContext _dbContext;
            private readonly IMailService _mailService;

            public Handler(
                AppDbContext dbContext,
                IMailService mailService)
            {
                _dbContext = dbContext;
                _mailService = mailService;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var rentalToDelete = await _dbContext.Rentals.FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    (request.SkipPhoneValidation || x.Phone == request.Phone));

                if (rentalToDelete == null)
                    throw new NotFoundException(ErrorCodes.Rentals.RENTAL_DOESNT_EXIST);

                _dbContext.Rentals.Attach(rentalToDelete);
                _dbContext.Rentals.Remove(rentalToDelete);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _mailService.SendRentalCanceledEmail(rentalToDelete);
            }
        }
    }
}