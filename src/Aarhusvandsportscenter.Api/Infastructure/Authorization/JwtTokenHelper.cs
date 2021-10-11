using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aarhusvandsportscenter.Api.Infastructure.Authorization
{
    public interface IJwtTokenHelper
    {
        JwtSecurityToken Token { get; set; }
        string GenerateJwtToken(AccountEntity account);
        IEnumerable<string> GetAudiences();
        string GetEmail();
        DateTime GetExpirationDate();
        string GetIssuer();
    }

    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly ILogger<JwtTokenHelper> _logger;
        private readonly AuthorizationSettings _authSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JwtSecurityToken Token { get; set; }

        public JwtTokenHelper(
            ILogger<JwtTokenHelper> logger,
            IOptions<AuthorizationSettings> authSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            this._logger = logger;
            this._authSettings = authSettings.Value;
            this._httpContextAccessor = httpContextAccessor;
        }

        public string GetEmail()
        {
            return Token.Claims.First(c => c.Type == CustomClaimTypes.Email).Value as string;
        }

        public string GetIssuer()
        {
            return Token.Issuer;
        }

        public IEnumerable<string> GetAudiences()
        {
            return Token.Audiences;
        }

        public DateTime GetExpirationDate()
        {
            return Token.ValidTo;
        }

        public string GenerateJwtToken(AccountEntity account)
        {
            _logger.LogInformation("Generating jwt token for account {0}", account.Id);

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _authSettings.Issuer,
                Audience = _authSettings.Audience,
                Claims = new Dictionary<string, object>(){
                    {CustomClaimTypes.Email, account.Email},
                    {CustomClaimTypes.FullName, account.FullName}
                },
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.AddSeconds(_authSettings.ExpirationInSeconds),
                SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}