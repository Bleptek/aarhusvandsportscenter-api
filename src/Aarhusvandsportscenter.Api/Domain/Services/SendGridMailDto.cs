using SendGrid.Helpers.Mail;

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    public class SendGridMailDto
    {
        public SendGridMailDto(
            string templateId,
            object templateData,
            EmailAddress from,
            EmailAddress to,
            EmailAddress replyTo = null)
        {
            TemplateId = templateId;
            TemplateData = templateData;
            From = from;
            To = to;
            ReplyTo = replyTo;
        }

        public string TemplateId { get; set; }
        public object TemplateData { get; set; }
        public EmailAddress From { get; set; }
        public EmailAddress To { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public EmailAddress ReplyTo { get; set; }
    }
}