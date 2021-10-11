using System.Collections.Generic;
using System.Linq;
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
    public static class UpdateContentPage
    {
        public record Command(string Key, string Title, IEnumerable<ContentPageSectionEntity> Sections) : IRequest<ContentPageEntity>;

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
                var existingEntry = await _dbContext.ContentPages
                    .Include(x => x.Sections)
                    .FirstOrDefaultAsync(x => x.Key == request.Key);
                if (existingEntry == null)
                    throw new NotFoundException(ErrorCodes.ContentPage.KEY_DOESNT_EXIST);

                existingEntry.Title = request.Title;

                var newSections = request.Sections.ToList();
                for (var i = 0; i < newSections.Count; i++)
                {
                    var existingSectionMatch = existingEntry.Sections.FirstOrDefault(x => x.Key == newSections[i].Key);
                    if (existingSectionMatch == null)
                        continue;

                    existingSectionMatch.MapFrom(newSections[i]);
                    newSections[i] = existingSectionMatch;
                }

                existingEntry.Sections = newSections;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return existingEntry;
            }
        }
    }
}