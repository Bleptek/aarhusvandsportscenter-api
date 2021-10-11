using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Accounts
{
    public static class LoginAccount
    {
        public record Command(string Email, string Password) : IRequest<AccountEntity>;

        public class Handler : IRequestHandler<Command, AccountEntity>
        {
            private readonly AppDbContext _dbContext;
            private readonly IPasswordService _passwordService;

            public Handler(
                AppDbContext dbContext,
                IPasswordService passwordService)
            {
                _dbContext = dbContext;
                _passwordService = passwordService;
            }

            public async Task<AccountEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (account == null)
                    throw new NotFoundException(ErrorCodes.Account.EMAIL_DOESNT_EXIST);

                if (string.IsNullOrEmpty(account.Password))
                    throw new BusinessRuleException(ErrorCodes.Account.PASSWORD_NOT_CREATED);

                if (_passwordService.VerifyPassword(request.Password, account.Salt, account.Password) == false)
                    throw new BusinessRuleException(ErrorCodes.Account.PASSWORD_INVALID);

                return account;
            }
        }
    }
}