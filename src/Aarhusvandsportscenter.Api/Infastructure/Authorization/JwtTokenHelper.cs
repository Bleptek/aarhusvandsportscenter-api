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
using Microsoft.Net.Http.Headers;

namespace Aarhusvandsportscenter.Api.Infastructure.Authorization
{
    public interface IJwtTokenHelper
    {
        string GenerateJwtToken(AccountEntity account);
        bool IsTokenValid();
    }

    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly ILogger<JwtTokenHelper> _logger;
        private readonly AuthorizationSettings _authSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenHelper(
            ILogger<JwtTokenHelper> logger,
            IOptions<AuthorizationSettings> authSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            this._logger = logger;
            this._authSettings = authSettings.Value;
            this._httpContextAccessor = httpContextAccessor;
        }

        private string GetToken()
        {
            var authHeaderStr = _httpContextAccessor.HttpContext!.Request.Headers[HeaderNames.Authorization];
            if (authHeaderStr.Count == 0)
                return null;

            var jwtTokenStr = authHeaderStr.ToString().Replace("Bearer ", "");
            return jwtTokenStr;
        }

        public bool IsTokenValid()
        {
            var token = GetToken();
            if (token == null)
                return false;

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = _authSettings.Audience,
                ValidIssuer = _authSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtKey))
            };

            try
            {
                handler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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