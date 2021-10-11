using System;
using System.Net.Http;
using Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Aarhusvandsportscenter.Api.Infastructure.Authorization;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aarhusvandsportscenter.Api.Tests.TestUtils
{
    /// <summary>
    /// Used to create a fake instance of the API.
    /// This is done during testing.
    /// </summary>
    public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        public CustomWebApplicationFactory()
        {
        }

        public string GenerateJwtToken(AccountEntity account = null)
        {
            var acc = new AccountEntity("Donald Trump", "donaltrump@gmail.com"); // will not be written to db. this is just a fake account object to create a token.
            var jwtTokenHelper = Services.CreateScope().ServiceProvider.GetService<IJwtTokenHelper>();
            var token = jwtTokenHelper.GenerateJwtToken(acc);
            return token;
        }

        public IServiceProvider GetScopedServiceProvider() => Services.CreateScope().ServiceProvider;

        public HttpClient CreateNewHttpClient(bool withBearer = false)
        {
            var httpClient = Server.CreateClient();
            if (withBearer)
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {GenerateJwtToken()}");

            return httpClient;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration which happened during Startup.cs. Override it for testing.
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                var dbName = Guid.NewGuid().ToString();
                // Add ApplicationDbContext using an in-memory database for testing.
                services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                        options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)); // to avoid transaction errors
                    });

                services.RemoveAll(typeof(IMailService));
                services.AddScoped<IMailService, STUB_Mailservice>();
            });
        }
    }
}