namespace Aarhusvandsportscenter.Api
{
    public class Appsettings
    {
        public string ASPNETCORE_ENVIRONMENT { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public SendGridSettings SendGrid { get; set; }
        public AuthorizationSettings Authorization { get; set; }
        public DefaultAdminAccount[] DefaultAdminAccounts { get; set; } = new DefaultAdminAccount[0];
        public bool CreateDefaultAdminAccounts { get; set; }

        public RentalSettings Rental { get; set; }
    }

    public class ConnectionStrings
    {
        public string DbConnection { get; set; }
    }

    public class SendGridSettings
    {
        public string ApiKey { get; set; }
        public string SendFromName { get; set; }
        public string SendFromEmail { get; set; }
        public string ResetPasswordTemplateId { get; set; }
        public string ResetPasswordLink { get; set; }
        public string RentalConfirmationTemplateId { get; set; }
        public string RentalCancellationTemplateId { get; set; }
        public string RentalCancellationLink { get; set; }
        public string RentalFinishLink { get; set; }
        public string ContactTemplateId { get; set; }
        public string ContactMailToName { get; set; }
        public string ContactMailToEmail { get; set; }
    }

    public class AuthorizationSettings
    {
        public string JwtKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationInSeconds { get; set; }
    }

    public class DefaultAdminAccount
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class RentalSettings
    {
        public string DefaultCategoryName { get; set; }
    }

}