using System;
using System.Linq;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    public class SendGridService : IMailService
    {
        private readonly ILogger<SendGridService> _logger;
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridSettings _sendGridSettings;

        public SendGridService(
            ILogger<SendGridService> logger,
            IOptions<SendGridSettings> sendGridSettings,
            ISendGridClient sendGridClient)
        {
            _logger = logger;
            _sendGridClient = sendGridClient;
            _sendGridSettings = sendGridSettings.Value;
        }

        /// <inheritdoc/>
        public async Task SendRentalConfirmationEmail(RentalEntity rental, decimal totalPrice)
        {
            var dto = new SendGridMailDto(
                templateId: _sendGridSettings.RentalConfirmationTemplateId,
                templateData: new
                {
                    rentalId = rental.Id,
                    paymentMethod = rental.PaymentMethod.ToString(),
                    dealCoupon = rental.DealCoupon,
                    fullName = rental.FullName,
                    items = rental.Items
                        .Select(x => new
                        {
                            count = x.Count,
                            name = x.Product.Name
                        })
                        .ToArray(),
                    startDate = rental.StartDate,
                    endDate = rental.EndDate,
                    totalPrice = totalPrice,
                    cancellationLink = _sendGridSettings.RentalCancellationLink.Replace("{id}", rental.Id.ToString()),
                    finishLink = _sendGridSettings.RentalFinishLink
                        .Replace("{id}", rental.Id.ToString())
                        .Replace("{phone}", rental.Phone), // this might be prone to errors if a phonenumber contains "+" and stuff
                },
                from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
                to: new EmailAddress(rental.EmailAddress, rental.FullName),
                replyTo: null
            );

            await SendEmail(dto);
        }

        /// <inheritdoc/>
        public async Task SendRentalCanceledEmail(RentalEntity rental)
        {
            var dto = new SendGridMailDto(
                templateId: _sendGridSettings.RentalCancellationTemplateId,
                templateData: new
                {
                    rentalId = rental.Id,
                    fullName = rental.FullName,
                    startDate = rental.StartDate,
                    endDate = rental.EndDate
                },
                from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
                to: new EmailAddress(rental.EmailAddress, rental.FullName),
                replyTo: null
            );

            await SendEmail(dto);
        }

        public async Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken)
        {
            var dto = new SendGridMailDto(
                templateId: _sendGridSettings.ResetPasswordTemplateId,
                templateData: new
                {
                    fullName = fullName,
                    resetLink = _sendGridSettings.ResetPasswordLink.Replace("{passwordToken}", resetPasswordToken.ToString())
                },
                from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
                to: new EmailAddress(email, fullName),
                replyTo: null
            );

            await SendEmail(dto);
        }

        public async Task SendContactEmail(string fromEmail, string fullName, string comment)
        {
            var dto = new SendGridMailDto(
                templateId: _sendGridSettings.ContactTemplateId,
                templateData: new
                {
                    fullName = fullName,
                    email = fromEmail,
                    comment = comment
                },
                from: new EmailAddress(_sendGridSettings.SendFromEmail, fullName),
                to: new EmailAddress(_sendGridSettings.ContactMailToEmail, _sendGridSettings.ContactMailToName),
                replyTo: new EmailAddress(fromEmail, fullName)
            );

            await SendEmail(dto);
        }

        protected virtual async Task SendEmail(SendGridMailDto mailDto)
        {
            var msg = new SendGridMessage();
            msg.AddTo(mailDto.To.Email, mailDto.To.Name);
            if (mailDto.ReplyTo != null)
                msg.SetReplyTo(new EmailAddress(mailDto.ReplyTo.Email, mailDto.ReplyTo.Name));
            msg.SetFrom(mailDto.From.Email, mailDto.From.Name);
            msg.SetTemplateId(mailDto.TemplateId);
            msg.SetTemplateData(mailDto.TemplateData);

            var response = await _sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid invoked sending to with template {msg.TemplateId}, response {content} {response.Body}");
            }
        }
    }
}