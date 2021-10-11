using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Tests.TestUtils;
using System;
using Aarhusvandsportscenter.Api.Controllers.Accounts;
using Aarhusvandsportscenter.Api.Infastructure.Database;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class AccountsControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public AccountsControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Account_FullSystemTest()
        {
            // Arrange
            var email = "someemail@asd.com";
            var fullName = "donald trump";
            var passwordFirst = "somepassword12312";
            HttpClient httpClient = null;

            // Act
            // 1. Create Account
            httpClient = _factory.CreateNewHttpClient(true);
            var createAccountResponse = await httpClient.PostAsync($"/api/v1/accounts",
                new CreateAccountRequest
                {
                    Email = email,
                    FullName = fullName
                }.ToHttpStringContent());

            // 2. Update Password
            Guid? pwResetToken = null;
            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdAccount = await appDbContext.Accounts.FirstAsync();
                pwResetToken = createdAccount.ResetPasswordToken;
            }

            httpClient = _factory.CreateNewHttpClient(false);
            var updatePasswordResponse = await httpClient.PutAsync($"/api/v1/accounts/updatePassword",
                new UpdateAccountPasswordRequest
                {
                    Password = passwordFirst,
                    ResetPasswordToken = pwResetToken.Value
                }.ToHttpStringContent());

            // 3. Login
            httpClient = _factory.CreateNewHttpClient(false);
            var loginResponse = await httpClient.PostAsync($"/api/v1/accounts/login",
                new LoginAccountRequest
                {
                    Email = email,
                    Password = passwordFirst
                }.ToHttpStringContent());

            // 4. Reset password
            httpClient = _factory.CreateNewHttpClient(false);
            var resetPwResponse = await httpClient.PostAsync($"/api/v1/accounts/resetPassword",
                new ResetAccountPasswordRequest
                {
                    Email = email
                }.ToHttpStringContent());

            // Assert
            Assert.Equal(HttpStatusCode.Created, createAccountResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, updatePasswordResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, resetPwResponse.StatusCode);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdAccount = await appDbContext.Accounts.FirstAsync(x => x.Email == email);
                Assert.Equal(fullName, createdAccount.FullName);
                Assert.Equal(email, createdAccount.Email);
                Assert.NotNull(createdAccount.Password);
                Assert.NotNull(createdAccount.Salt);

                var loginResponseObj = await loginResponse.DeserializeHttpResponse<LoginResponse>();
                Assert.NotNull(loginResponseObj.Jwt);
            }
        }
    }
}