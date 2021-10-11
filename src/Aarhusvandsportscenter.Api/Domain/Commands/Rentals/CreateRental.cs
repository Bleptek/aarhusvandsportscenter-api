using System.Threading;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using System.Linq;
using Aarhusvandsportscenter.Api.Domain.Exceptions;

namespace Aarhusvandsportscenter.Api.Domain.Commands.Rentals
{
    public static class CreateRental
    {
        public record Command(RentalEntity Rental) : IRequest<RentalEntity>;

        public class Handler : IRequestHandler<Command, RentalEntity>
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

            public async Task<RentalEntity> Handle(Command request, CancellationToken cancellationToken)
            {
                var defaultCategory = await _dbContext.RentalCategories.FirstAsync(x => x.IsDefault);
                var allProducts = await _dbContext.RentalProducts.Include(x => x.Prices).ToArrayAsync();
                var existingRentalsSpanningPeriod = await GetRentalsSpanningPeriod(request);

                ValidateAvailableProduct(request.Rental, allProducts, existingRentalsSpanningPeriod);
                var totalPrice = CalculateTotalPrice(request.Rental, allProducts);
                request.Rental.Category = defaultCategory;

                await _dbContext.Rentals.AddAsync(request.Rental);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await LoadStuffForEmail(request.Rental);

                await _mailService.SendRentalConfirmationEmail(request.Rental, totalPrice);

                return request.Rental;
            }

            private async Task LoadStuffForEmail(RentalEntity rental) =>
                await _dbContext
                    .Entry(rental)
                    .Collection(x => x.Items)
                    .Query()
                    .Include(x => x.Product)
                    .LoadAsync();

            private async Task<RentalEntity[]> GetRentalsSpanningPeriod(Command request) => 
                await _dbContext.Rentals
                    .Include(x => x.Items)
                    .Where(x => x.StartDate.Date <= request.Rental.EndDate.Date && x.EndDate.Date >= request.Rental.StartDate.Date)
                    .ToArrayAsync();

            private static decimal CalculateTotalPrice(RentalEntity newRental, RentalProductEntity[] allProducts)
            {
                var totalDays = (newRental.EndDate - newRental.StartDate).Days +1; // +1 because both days are inclusive
                var totalPrice = 0m;
                foreach (var item in newRental.Items)
                {
                    var product = allProducts.FirstOrDefault(x => x.Id == item.ProductId);
                    if (product == null)
                        throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                    var volume = totalDays * item.Count;
                    var volumePrice = product.Prices.Where(x => x.Quantity <= volume).OrderByDescending(x => x.Quantity).First().UnitPrice;
                    totalPrice += volumePrice * volume;
                }

                return totalPrice;
            }

            private static void ValidateAvailableProduct(RentalEntity newRental, RentalProductEntity[] allProducts, RentalEntity[] existingRentalsSpanningPeriod)
            {
                foreach (var item in newRental.Items)
                {
                    var product = allProducts.FirstOrDefault(x => x.Id == item.ProductId);
                    if (product == null)
                        throw new NotFoundException(ErrorCodes.Rentals.PRODUCT_DOESNT_EXIST);

                    var currDate = newRental.StartDate.Date;
                    while (currDate <= newRental.EndDate.Date)
                    {
                        var existingRentalsProductSum = existingRentalsSpanningPeriod
                            .Where(x => x.StartDate.Date <= currDate.Date && x.EndDate.Date >= currDate.Date)
                            .SelectMany(x => x.Items)
                            .Where(x => x.ProductId == product.Id)
                            .Sum(x => x.Count);

                        if (product.AmountInStock - (existingRentalsProductSum + item.Count) < 0)
                            throw new BusinessRuleException(ErrorCodes.Rentals.EXCEEDING_AVAILABLE_PRODUCT);

                        currDate = currDate.AddDays(1);
                    }
                }
            }
        }
    }
}