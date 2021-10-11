using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;

namespace Aarhusvandsportscenter.Api.Domain.Commands.ContentPages
{
    public static class UploadContentPageImage
    {
        public record Command(string PageKey, IFormFile ImageFile) : IRequest<ContentPageImageEntity>;

        public class Handler : IRequestHandler<Command, ContentPageImageEntity>
        {
            private readonly AppDbContext _dbContext;
            private readonly ILogger<Handler> _logger;

            public Handler(AppDbContext dbContext, ILogger<Handler> logger)
            {
                _dbContext = dbContext;
                _logger = logger;
            }

            public async Task<ContentPageImageEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var contentPage = await _dbContext.ContentPages.FirstOrDefaultAsync(x => x.Key == request.PageKey);
                if (contentPage == null)
                    throw new NotFoundException(ErrorCodes.ContentPage.KEY_DOESNT_EXIST);

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                var publicPath = await StoreImageFile(request.PageKey, request.ImageFile);

                var imageEntity = new ContentPageImageEntity(publicPath)
                {
                    ContentPage = contentPage
                };
                await _dbContext.ContentPageImages.AddAsync(imageEntity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return imageEntity;
            }

            /// <summary>
            /// </summary>
            /// <returns>filepath</returns>
            private async Task<string> StoreImageFile(string pageKey, IFormFile file)
            {
                var fileName = $"{pageKey}-{Guid.NewGuid().ToString().Substring(0, 8)}.{file.FileName.Split(".").Last()}";
                var publicFolderPath = $"images/contentPages";
                var internalFolderPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{publicFolderPath}");



                if (!System.IO.Directory.Exists(internalFolderPath))
                    System.IO.Directory.CreateDirectory(internalFolderPath);

                using (var stream = System.IO.File.Create($"{internalFolderPath}/{fileName}"))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{publicFolderPath}/{fileName}";
            }
        }
    }
}