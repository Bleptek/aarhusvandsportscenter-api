using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Domain.Queries.ContentPages
{
    public static class GetContentPages
    {
        // query
        public record Query() : IRequest<IEnumerable<ContentPageEntity>>;

        // handler
        public class Handler : IRequestHandler<Query, IEnumerable<ContentPageEntity>>
        {
            private readonly AppDbContext _dbContext;

            public Handler(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<IEnumerable<ContentPageEntity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var contentPages = await _dbContext.ContentPages
                    .AsNoTracking()
                    .Include(x => x.Sections)
                    .ToArrayAsync();
                return contentPages;
            }
        }
    }
}