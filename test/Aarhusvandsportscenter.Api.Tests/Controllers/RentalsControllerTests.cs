using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Collections.Generic;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Controllers.Rentals;
using Aarhusvandsportscenter.Api.Controllers.RentalCategories;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class RentalsControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public RentalsControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetRentals_EndpointSuccessTest()
        {
            // Arrange
            var startDate = new DateTime(2021, 1, 25);
            var endDate = new DateTime(2021, 2, 10);

            var category = new RentalCategoryEntity("test1", "red", true);
            var product = new RentalProductEntity("kajak", "kajakker", 5);
            Func<List<RentalItemEntity>> items = () => new List<RentalItemEntity>(){
                new RentalItemEntity{
                    Count = 5,
                    Product = product
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.Rentals.AddRange(new List<RentalEntity>(){
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20)){Category = category,Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 25), new DateTime(2021, 1, 25)){Category = category,Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 29), new DateTime(2021, 1, 29)){Category = category,Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 2, 1), new DateTime(2021, 2, 1)){Category = category,Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 2, 10), new DateTime(2021, 2, 10)){Category = category,Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 2, 11), new DateTime(2021, 2, 11)){Category = category,Items = items()},
                });
                appDbContext.SaveChanges();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient();
            var httpResponse = await httpClient.GetAsync($"/api/v1/rentals?startDate={startDate.ToString("yyyy-MM-dd")}&endDate={endDate.ToString("yyyy-MM-dd")}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<IEnumerable<RentalCompactResponse>>();
            Assert.NotEmpty(responseObj);

            Assert.All(responseObj, x =>
            {
                Assert.True(x.EndDate.ToUniversalTime().Date >= startDate.ToUniversalTime().Date);
                Assert.True(x.StartDate.ToUniversalTime().Date <= endDate.ToUniversalTime().Date);
                Assert.Single(x.Items);
                Assert.NotNull(x.Items.First().ProductName);
            });
        }

        [Fact]
        public async Task GetRental_EndpointSuccessTest()
        {
            // Arrange
            var category = new RentalCategoryEntity("test1", "red", true);
            var product = new RentalProductEntity("kajak", "kajakker", 5);
            Func<List<RentalItemEntity>> items = () => new List<RentalItemEntity>(){
                new RentalItemEntity{
                    Count = 5,
                    Product = product
                }
            };

            var rental = new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20)){Category = category,Items = items()};
            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.Rentals.Add(rental);
                appDbContext.SaveChanges();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient();
            var httpResponse = await httpClient.GetAsync($"/api/v1/rentals/{rental.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalCompactResponse>();
            Assert.Equal(rental.Id, responseObj.Id);
        }

        [Fact]
        public async Task CreateRental_EndpointSuccessTest()
        {
            // Arrange
            var existingProducts = new List<RentalProductEntity>(){
                new RentalProductEntity("kajak", "kajakker", 5){
                    Prices = new List<RentalProductPriceEntity>{
                        new RentalProductPriceEntity{
                            UnitPrice = 50,
                            Quantity = 1,
                        }
                    }
                },
                new RentalProductEntity("kano", "kanoer", 5){
                    Prices = new List<RentalProductPriceEntity>{
                        new RentalProductPriceEntity{
                            UnitPrice = 50,
                            Quantity = 1,
                        }
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.AddRange(existingProducts);
                appDbContext.RentalCategories.Add(new RentalCategoryEntity("test", "red", true));
                appDbContext.SaveChanges();
            }


            var request = new CreateRentalRequest
            {
                DealCoupon = "someDeal",
                DealSite = "SpotDeal",
                EmailAddress = "someemail@example.com",
                FullName = "some name",
                Items = new List<CreateRentalItemRequest>(){
                    new CreateRentalItemRequest{
                        Count = 3,
                        ProductId = existingProducts.ElementAt(0).Id
                    },
                    new CreateRentalItemRequest{
                        Count = 5,
                        ProductId = existingProducts.ElementAt(1).Id
                    }
                },
                PaymentMethod = PaymentMethodEnum.MobilePay,
                Phone = "+45 12 12 12 12",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(5)
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.PostAsync($"/api/v1/rentals", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalCompactResponse>();
            Assert.Equal(request.StartDate.ToUniversalTime().ToString(), responseObj.StartDate.ToUniversalTime().ToString());
            Assert.Equal(request.EndDate.ToUniversalTime().ToString(), responseObj.EndDate.ToUniversalTime().ToString());

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdRental = await appDbContext.Rentals
                    .Include(x => x.Items)
                        .ThenInclude(i => i.Product)
                    .FirstAsync(x => x.Id == responseObj.Id);

                Assert.Equal(request.DealCoupon, createdRental.DealCoupon);
                Assert.Equal(request.DealSite, createdRental.DealSite);
                Assert.Equal(request.EmailAddress, createdRental.EmailAddress);
                Assert.Equal(request.FullName, createdRental.FullName);
                Assert.Equal(request.PaymentMethod, createdRental.PaymentMethod);
                Assert.Equal(request.Phone, createdRental.Phone);

                Assert.Equal(2, createdRental.Items.Count());
                Assert.Equal(existingProducts.ElementAt(0).Name, createdRental.Items.ElementAt(0).Product.Name);
                Assert.Equal(request.Items.ElementAt(0).Count, createdRental.Items.ElementAt(0).Count);

                Assert.Equal(existingProducts.ElementAt(1).Name, createdRental.Items.ElementAt(1).Product.Name);
                Assert.Equal(request.Items.ElementAt(1).Count, createdRental.Items.ElementAt(1).Count);
            }
        }

        [Fact]
        public async Task CreateRental_Fails_WhenRentalExeedsAvailableProduct()
        {
            // Arrange
            var existingProducts = new List<RentalProductEntity>(){
                new RentalProductEntity("kajak", "kajakker", 5),
                new RentalProductEntity("kano", "kanoer", 5)
            };

            var existingRentals = new List<RentalEntity>(){
                new RentalEntity("asd", "123123", "thoams@example.com", DateTime.Today.AddDays(5), DateTime.Today.AddDays(5)){
                    Items = new List<RentalItemEntity>(){
                        new RentalItemEntity(){
                            Product = existingProducts[1],
                            Count = 4
                        }
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.AddRange(existingProducts);
                appDbContext.Rentals.AddRange(existingRentals);
                appDbContext.RentalCategories.Add(new RentalCategoryEntity("test", "red", true));
                appDbContext.SaveChanges();
            }


            var request = new CreateRentalRequest
            {
                DealCoupon = "someDeal",
                DealSite = "SpotDeal",
                EmailAddress = "someemail@example.com",
                FullName = "some name",
                Items = new List<CreateRentalItemRequest>(){
                    new CreateRentalItemRequest{
                        Count = 2,
                        ProductId = existingProducts[1].Id
                    }
                },
                PaymentMethod = PaymentMethodEnum.MobilePay,
                Phone = "+45 12 12 12 12",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(5),
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.PostAsync($"/api/v1/rentals", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, httpResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteRental_EndpointSuccessTest()
        {
            // Arrange
            var phoneNumber = "12351287653";
            var rentalToDelete = new RentalEntity("thomas", phoneNumber, "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20))
            {
                Category = new RentalCategoryEntity("test", "red", true),
                Items = new List<RentalItemEntity>(){
                    new RentalItemEntity{
                        Count = 5,
                        Product = new RentalProductEntity("kajak", "kajakker", 5)
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.Rentals.Add(rentalToDelete);
                appDbContext.SaveChanges();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.DeleteAsync($"/api/v1/rentals/{rentalToDelete.Id}?phone={phoneNumber}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var deletedRental = await appDbContext.Rentals.FirstOrDefaultAsync(x => x.Id == rentalToDelete.Id);
                Assert.Null(deletedRental);
            }
        }

        [Fact]
        public async Task FinishRental_EndpointSuccessTest()
        {
            // Arrange
            var phoneNumber = "12351287653";
            var rentalToFinish = new RentalEntity("thomas", phoneNumber, "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20))
            {
                Category = new RentalCategoryEntity("test", "red", true),
                Items = new List<RentalItemEntity>(){
                    new RentalItemEntity{
                        Count = 5,
                        Product = new RentalProductEntity("kajak", "kajakker", 5)
                    }
                },
                Done = false
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.Rentals.Add(rentalToFinish);
                appDbContext.SaveChanges();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.PutAsync($"/api/v1/rentals/{rentalToFinish.Id}/finish?phone={phoneNumber}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedRental = await appDbContext.Rentals.FirstOrDefaultAsync(x => x.Id == rentalToFinish.Id);
                Assert.True(updatedRental.Done);
            }
        }

        [Fact]
        public async Task UpdateRental_EndpointSuccessTest()
        {
            // Arrange
            var categories = new List<RentalCategoryEntity>(){
                new RentalCategoryEntity(Guid.NewGuid().ToString(), "red", false),
                new RentalCategoryEntity(Guid.NewGuid().ToString(), "red", false),
            };
            var rental = new RentalEntity("thomas", "1231234123", "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20))
            {
                Category = categories[0],
                Items = new List<RentalItemEntity>(){
                    new RentalItemEntity{
                        Count = 5,
                        Product = new RentalProductEntity("kajak", "kajakker", 5)
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalCategories.AddRange(categories);
                appDbContext.Rentals.Add(rental);
                appDbContext.SaveChanges();
            }

            var request = new UpdateRentalRequest{
                CategoryId = categories[1].Id,
                Comment = "donald trump"
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/rentals/{rental.Id}", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedEntry = await appDbContext.Rentals.FirstOrDefaultAsync(x => x.Id == rental.Id);
                Assert.Equal(request.CategoryId, updatedEntry.CategoryId);
                Assert.Equal(request.Comment, updatedEntry.AdminComment);
            }
        }

        [Fact]
        public async Task GetRentalsDetailed_EndpointSuccessTest()
        {
            // Arrange
            var startDate = new DateTime(2021, 1, 25);
            var endDate = new DateTime(2021, 2, 10);

            var categories = new List<RentalCategoryEntity>(){
                new RentalCategoryEntity("ny2", "red", false),
                new RentalCategoryEntity("afvist2", "red", false),
                new RentalCategoryEntity("betalt2", "red", false)
            };
            var product = new List<RentalProductEntity>(){
                new RentalProductEntity("kajak", "kajakker", 5){
                    Prices = new List<RentalProductPriceEntity>(){
                        new RentalProductPriceEntity(5, 100)
                    }
                }
            };


            Func<List<RentalItemEntity>> items = () => new List<RentalItemEntity>(){
                new RentalItemEntity{
                    Count = 5,
                    Product = product[0]
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.Rentals.AddRange(new List<RentalEntity>(){
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 20), new DateTime(2021, 1, 20)){Category = categories[0],Items = items()}, // excluded date
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 25), new DateTime(2021, 1, 25)){Category = categories[0],Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 29), new DateTime(2021, 1, 29)){Category = categories[1],Items = items()},
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 2, 1), new DateTime(2021, 2, 1)){Category = categories[1],Items = items()}, // excluded date
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 1, 25), new DateTime(2021, 1, 25)){Category = categories[2],Items = items()}, // excluded category
                    new RentalEntity("thomas", "11111111", "asd@mail.com", new DateTime(2021, 2, 11), new DateTime(2021, 2, 11)){Category = categories[2],Items = items()}, // excluded category
                });
                appDbContext.SaveChanges();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var getUrl = $"/api/v1/rentals/detailed";
            getUrl += $"?categoryId[in]={categories[0].Id},{categories[1].Id}";
            getUrl += $"&startDate[gt]=2021-01-22&startDate[lt]=2021-01-31";
            Console.WriteLine(getUrl);
            var httpResponse = await httpClient.GetAsync(getUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<IEnumerable<RentalResponse>>();
            Assert.NotEmpty(responseObj);


            Assert.Equal(2, responseObj.Count());
            Assert.All(responseObj, x =>
            {
                Assert.Single(x.Items);
                Assert.NotNull(x.Items.First().ProductName);
            });
        }
    }
}