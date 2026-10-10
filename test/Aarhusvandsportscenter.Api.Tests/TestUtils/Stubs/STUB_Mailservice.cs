using System.Collections.Generic;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api;
using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs
{
    /// <summary>
    /// A <see cref="SmtpMailService"/> that never actually talks to an SMTP server. Instead it records every
    /// email that would have been sent, so tests can assert on subject/body/recipients without network access.
    /// </summary>
    public class STUB_Mailservice : SmtpMailService
    {
        public record SentEmail(string Subject, string BodyHtml, string ToEmail, string FromName, string ReplyToEmail, string ReplyToName);

        public List<SentEmail> SentEmails { get; } = new List<SentEmail>();

        public STUB_Mailservice(
            ILogger<SmtpMailService> logger,
            IOptions<SimplySmtpSettings> simplySmtpSettings)
            : base(logger, simplySmtpSettings)
        {
        }

        protected override async Task SendEmailUsingSmtp(
            string subject,
            string bodyHtml,
            string toEmail,
            string fromName = null,
            string replyToEmail = null,
            string replyToName = null)
        {
            SentEmails.Add(new SentEmail(subject, bodyHtml, toEmail, fromName, replyToEmail, replyToName));
            await Task.CompletedTask;
        }
    }
}