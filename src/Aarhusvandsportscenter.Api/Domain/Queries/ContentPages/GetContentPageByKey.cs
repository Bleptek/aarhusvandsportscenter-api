using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Queries.ContentPages
{
    public static class GetContentPageByKey
    {
        // query
        public record Query(string Key) : IRequest<ContentPageEntity>;

        // handler
        public class Handler : IRequestHandler<Query, ContentPageEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<ContentPageEntity> Handle(Query request, CancellationToken cancellationToken)
            {
                var contentPage = await _dbContext.ContentPages
                    .AsNoTracking()
                    .Include(x => x.Sections)
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.Key == request.Key);
                if (contentPage == null)
                    throw new NotFoundException(ErrorCodes.ContentPage.KEY_DOESNT_EXIST);

                return contentPage;
            }
        }
    }
}