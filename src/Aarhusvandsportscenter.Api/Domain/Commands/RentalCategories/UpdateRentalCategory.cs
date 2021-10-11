using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Domain.Commands.RentalCategories
{
    public static class UpdateRentalCategory
    {
        public record Command(int Id, string Name, string ColorCode, bool IsDefault = false) : IRequest<RentalCategoryEntity>;

        public class Handler : IRequestHandler<Command, RentalCategoryEntity>
        {
            private readonly AppDbContext _dbContext;

            public Handler(
                AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RentalCategoryEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingCategory = await _dbContext.RentalCategories.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (existingCategory == null)
                    throw new NotFoundException(ErrorCodes.Rentals.CATEGORY_DOESNT_EXIST);

                existingCategory.Name = request.Name;
                existingCategory.ColorCode = request.ColorCode;
                if(request.IsDefault){
                    var defaultCategory = await _dbContext.RentalCategories.FirstOrDefaultAsync(x => x.IsDefault == true);
                    if(defaultCategory != null)
                        defaultCategory.IsDefault = false;
                    existingCategory.IsDefault = true;
                }

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException e) // cause Name is unique
                {
                    if (e.InnerException is MySqlException mysqlEx &&
                        mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                        throw new ConflictException(ErrorCodes.Rentals.CATEGORY_CONFLICT);

                    throw;
                }

                return existingCategory;
            }
        }
    }
}