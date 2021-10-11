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
using Aarhusvandsportscenter.Api.Controllers.RentalProducts;
using Aarhusvandsportscenter.Api.Controllers.RentalProductPrices;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class RentalProductPricesControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public RentalProductPricesControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateRentalProductPrice_EndpointSuccessTest()
        {
            // Arrange
            var existingProduct = new RentalProductEntity("kajak", "kajakker", 10);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.Add(existingProduct);
                await appDbContext.SaveChangesAsync();
            }

            var request = new RentalProductPriceRequest
            {
                Quantity = 1,
                UnitPrice = 50
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PostAsync($"/api/v1/rentalProducts/{existingProduct.Id}/prices", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalProductPriceResponse>();
            Assert.Equal(existingProduct.Id, responseObj.ProductId);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdEntry = await appDbContext.RentalProducts
                    .Include(x => x.Prices)
                    .FirstAsync(x => x.Id == existingProduct.Id);

                Assert.Single(createdEntry.Prices);

                Assert.Equal(request.Quantity, createdEntry.Prices.ElementAt(0).Quantity);
                Assert.Equal(request.UnitPrice, createdEntry.Prices.ElementAt(0).UnitPrice);
            }
        }

        [Fact]
        public async Task UpdateRentalProductPrice_EndpointSuccessTest()
        {
            // Arrange
            var existingProduct = new RentalProductEntity("kajak", "kajakker", 10)
            {
                Prices = new List<RentalProductPriceEntity>(){
                    new RentalProductPriceEntity(1, 100),
                    new RentalProductPriceEntity(3, 150)
                },
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.Add(existingProduct);
                await appDbContext.SaveChangesAsync();
            }

            var priceId = existingProduct.Prices.ElementAt(1).Id;

            var request = new RentalProductPriceRequest
            {
                Quantity = 3,
                UnitPrice = 75
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/rentalProducts/{existingProduct.Id}/prices/{priceId}", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalProductPriceResponse>();
            Assert.Equal(existingProduct.Id, responseObj.ProductId);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedEntry = await appDbContext.RentalProducts
                    .Include(x => x.Prices)
                    .FirstAsync(x => x.Id == existingProduct.Id);

                Assert.Equal(2, updatedEntry.Prices.Count());
                Assert.Equal(request.Quantity, updatedEntry.Prices.ElementAt(1).Quantity);
                Assert.Equal(request.UnitPrice, updatedEntry.Prices.ElementAt(1).UnitPrice);
            }
        }

        [Fact]
        public async Task DeleteRentalProductPrice_EndpointSuccessTest()
        {
            // Arrange
            var existingProduct = new RentalProductEntity("kajak", "kajakker", 10)
            {
                Prices = new List<RentalProductPriceEntity>(){
                    new RentalProductPriceEntity(1, 100),
                    new RentalProductPriceEntity(2, 100)
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.Add(existingProduct);
                await appDbContext.SaveChangesAsync();
            }

            var priceId = existingProduct.Prices.ElementAt(1).Id;

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.DeleteAsync($"/api/v1/rentalProducts/{existingProduct.Id}/prices/{priceId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedEntry = await appDbContext.RentalProducts
                    .Include(x => x.Prices)
                    .FirstAsync(x => x.Id == existingProduct.Id);

                Assert.Single(updatedEntry.Prices);
            }
        }
    }
}