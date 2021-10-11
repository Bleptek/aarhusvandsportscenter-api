using System;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Accounts
{
    public static class ResetAccountPassword
    {
        public record Command(string Email) : IRequest;

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
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (account == null)
                    throw new NotFoundException(ErrorCodes.Account.EMAIL_DOESNT_EXIST);

                var resetToken = Guid.NewGuid();
                account.ResetPasswordToken = resetToken;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _mailService.SendResetPasswordEmail(account.Email, account.FullName, resetToken);
            }
        }
    }
}