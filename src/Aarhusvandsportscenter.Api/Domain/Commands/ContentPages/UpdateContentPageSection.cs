using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.ContentPages
{
    public static class UpdateContentPageSection
    {
        public record Command(string PageKey, ContentPageSectionEntity Section) : IRequest<ContentPageSectionEntity>;

        public class Handler : IRequestHandler<Command, ContentPageSectionEntity>
        {
            private readonly AppDbContext _dbContext;
            private readonly ILogger<Handler> _logger;

            public Handler(AppDbContext dbContext, ILogger<Handler> logger)
            {
                _dbContext = dbContext;
                _logger = logger;
            }

            public async Task<ContentPageSectionEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingEntry = await _dbContext.ContentPageSections
                    .FirstOrDefaultAsync(x => x.ContentPage.Key == request.PageKey && x.Key == request.Section.Key);
                if (existingEntry == null)
                    throw new NotFoundException(ErrorCodes.ContentPage.KEY_DOESNT_EXIST);

                existingEntry.MapFrom(request.Section);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return existingEntry;
            }
        }
    }
}