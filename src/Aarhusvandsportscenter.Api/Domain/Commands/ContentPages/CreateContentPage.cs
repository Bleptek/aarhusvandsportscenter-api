using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.ContentPages
{
    public static class CreateContentPage
    {
        public record Command(ContentPageEntity contentPage) : IRequest<ContentPageEntity>;

        public class Handler : IRequestHandler<Command, ContentPageEntity>
        {
            private readonly AppDbContext _dbContext;
            private readonly ILogger<Handler> _logger;

            public Handler(AppDbContext dbContext, ILogger<Handler> logger)
            {
                _dbContext = dbContext;
                _logger = logger;
            }

            public async Task<ContentPageEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                await _dbContext.ContentPages.AddAsync(request.contentPage);

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

                return request.contentPage;
            }
        }
    }
}