using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Queries.ContentPages
{
    public static class GetContentPageSection
    {
        // query
        public record Query(string PageKey, string SectionKey) : IRequest<ContentPageSectionEntity>;

        // handler
        public class Handler : IRequestHandler<Query, ContentPageSectionEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<ContentPageSectionEntity> Handle(Query request, CancellationToken cancellationToken)
            {
                var contentPageSection = await _dbContext.ContentPageSections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ContentPage.Key == request.PageKey && x.Key == request.SectionKey);
                if (contentPageSection == null)
                    throw new NotFoundException(ErrorCodes.ContentPage.KEY_DOESNT_EXIST);

                return contentPageSection;
            }
        }
    }
}