using System;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Accounts
{
    public static class CreateAccount
    {
        public record Command(string FullName, string Email) : IRequest<AccountEntity>;

        public class Handler : IRequestHandler<Command, AccountEntity>
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

            public async Task<AccountEntity> Handle(Command request, CancellationToken cancellationToken)
            {

                var newAccount = new AccountEntity(request.FullName, request.Email);
                newAccount.ResetPasswordToken = Guid.NewGuid();
                await _dbContext.Accounts.AddAsync(newAccount);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is MySqlException mysqlEx &&
                        mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                        throw new ConflictException(ErrorCodes.ContentPage.CONFLICT);

                    throw;
                }

                await _mailService.SendResetPasswordEmail(request.Email, request.FullName, newAccount.ResetPasswordToken.Value);

                return newAccount;
            }
        }
    }
}