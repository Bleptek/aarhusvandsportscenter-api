using System.Threading.Tasks;
using Aarhusvandsportscenter.Api;
using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs
{
    public class STUB_Mailservice : SmtpMailService
    {
        public STUB_Mailservice(
            ILogger<SmtpMailService> logger,
            IOptions<SimplySmtpSettings> simplySmtpSettings)
            : base(logger, simplySmtpSettings)
        {
        }


        protected override async Task SendEmailUsingSmtp(string subject, string bodyHtml, string toEmail)
        {
            await Task.Run(() => { });
        }
    }
}