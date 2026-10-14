using System.Net;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Tests.TestUtils;
using Xunit;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class SwaggerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public SwaggerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SwaggerDocument_IsGenerated_WithBearerSecurityScheme()
        {
            var httpClient = _factory.CreateNewHttpClient();

            var httpResponse = await httpClient.GetAsync("/swagger/v1/swagger.json");

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var json = await httpResponse.Content.ReadAsStringAsync();
            Assert.Contains("\"Bearer\"", json);
            Assert.Contains("/api/v1/Rentals", json);
        }
    }
}
