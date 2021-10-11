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

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class RentalProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public RentalProductsControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateRentalProduct_EndpointSuccessTest()
        {
            // Arrange
            var request = new RentalProductRequest
            {
                AmountInStock = 12,
                Name = "kajak",
                NamePlural = "kajakker"
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PostAsync($"/api/v1/rentalProducts", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalProductResponse>();
            Assert.Equal(request.Name, responseObj.Name);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdProduct = await appDbContext.RentalProducts
                    .FirstAsync(x => x.Id == responseObj.Id);

                Assert.Equal(request.Name, createdProduct.Name);
                Assert.Equal(request.AmountInStock, createdProduct.AmountInStock);
            }
        }

        [Fact]
        public async Task UpdateRentalProduct_EndpointSuccessTest()
        {
            // Arrange
            var existingProduct = new RentalProductEntity("kajak", "kajakker", 10);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.Add(existingProduct);
                await appDbContext.SaveChangesAsync();
            }

            var request = new RentalProductRequest
            {
                AmountInStock = 12,
                Name = "kajak",
                NamePlural = "kajakker"
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/rentalProducts/{existingProduct.Id}", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalProductResponse>();
            Assert.Equal(request.Name, responseObj.Name);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedProduct = await appDbContext.RentalProducts
                    .FirstAsync(x => x.Id == responseObj.Id);

                Assert.Equal(request.Name, updatedProduct.Name);
                Assert.Equal(request.AmountInStock, updatedProduct.AmountInStock);
            }
        }

        [Fact]
        public async Task GetRentalProducts_EndpointSuccessTest()
        {
            // Arrange
            var existingProducts = new List<RentalProductEntity>(){
                new RentalProductEntity("kajak", "kajakker", 10),
                new RentalProductEntity("kano", "kanoer", 15)
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.AddRange(existingProducts);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.GetAsync($"/api/v1/rentalProducts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<IEnumerable<RentalProductResponse>>();

            Assert.NotEmpty(responseObj);
            foreach (var p in existingProducts)
            {
                var match = responseObj.FirstOrDefault(x => x.Id == p.Id);
                Assert.NotNull(match);
                Assert.Equal(p.Name, match.Name);
                Assert.Equal(p.AmountInStock, match.AmountInStock);
            }
        }

        [Fact]
        public async Task DeleteRentalProduct_EndpointSuccessTest()
        {
            // Arrange
            var productToDelete = new RentalProductEntity("kajak", "kajakker", 10);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalProducts.Add(productToDelete);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.DeleteAsync($"/api/v1/rentalProducts/{productToDelete.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var deletedProduct = await appDbContext.RentalProducts.FirstOrDefaultAsync(x => x.Id == productToDelete.Id);
                Assert.Null(deletedProduct);
            }
        }
    }
}