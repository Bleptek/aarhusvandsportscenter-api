using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;

namespace Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs
{
    public class STUB_Mailservice : SendGridService
    {
        public STUB_Mailservice(
            ILogger<SendGridService> logger,
            IOptions<SendGridSettings> sendGridSettings,
            ISendGridClient sendGridClient)
            : base(logger, sendGridSettings, sendGridClient)
        {
        }


        protected override async Task SendEmail(SendGridMailDto mailDto)
        {
            await Task.Run(() => { });
        }
    }
}