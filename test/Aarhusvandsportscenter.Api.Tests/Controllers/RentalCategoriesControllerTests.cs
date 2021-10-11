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
using Aarhusvandsportscenter.Api.Controllers.RentalCategories;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class RentalCategoriesControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public RentalCategoriesControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateRentalCategory_EndpointSuccessTest()
        {
            // Arrange
            var request = new RentalCategoryRequest
            {
                IsDefault = false,
                ColorCode = "red",
                Name = "unpaid"
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PostAsync($"/api/v1/rentalCategories", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalCategoryResponse>();
            Assert.Equal(request.Name, responseObj.Name);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdObj = await appDbContext.RentalCategories
                    .FirstAsync(x => x.Id == responseObj.Id);

                Assert.Equal(request.Name, createdObj.Name);
                Assert.Equal(request.IsDefault, createdObj.IsDefault);
            }
        }

        [Fact]
        public async Task UpdateRentalCategory_EndpointSuccessTest()
        {
            // Arrange
            var existingProduct = new RentalCategoryEntity("Rød", "red", false);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalCategories.Add(existingProduct);
                await appDbContext.SaveChangesAsync();
            }

            var request = new RentalCategoryRequest
            {
                IsDefault = true,
                Name = "paid",
                ColorCode = "black"
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/rentalCategories/{existingProduct.Id}", request.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<RentalCategoryResponse>();
            Assert.Equal(request.Name, responseObj.Name);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedProduct = await appDbContext.RentalCategories
                    .FirstAsync(x => x.Id == responseObj.Id);

                Assert.Equal(request.Name, updatedProduct.Name);
                Assert.Equal(request.ColorCode, updatedProduct.ColorCode);
                Assert.Equal(request.IsDefault, updatedProduct.IsDefault);
            }
        }

        [Fact]
        public async Task GetRentalCategories_EndpointSuccessTest()
        {
            // Arrange
            var existingProducts = new List<RentalCategoryEntity>(){
                new RentalCategoryEntity("unpaid", "red", false),
                new RentalCategoryEntity("paid", "green", false)
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalCategories.AddRange(existingProducts);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.GetAsync($"/api/v1/rentalCategories");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<IEnumerable<RentalCategoryResponse>>();

            Assert.NotEmpty(responseObj);
            foreach (var p in existingProducts)
            {
                var match = responseObj.FirstOrDefault(x => x.Id == p.Id);
                Assert.NotNull(match);
                Assert.Equal(p.Name, match.Name);
                Assert.Equal(p.ColorCode, match.ColorCode);
                Assert.Equal(p.IsDefault, match.IsDefault);
            }
        }

        [Fact]
        public async Task DeleteRentalCategory_EndpointSuccessTest()
        {
            // Arrange
            var productToDelete = new RentalCategoryEntity("halfpaid", "yellow", false);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.RentalCategories.Add(productToDelete);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.DeleteAsync($"/api/v1/rentalCategories/{productToDelete.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var deletedProduct = await appDbContext.RentalCategories.FirstOrDefaultAsync(x => x.Id == productToDelete.Id);
                Assert.Null(deletedProduct);
            }
        }
    }
}