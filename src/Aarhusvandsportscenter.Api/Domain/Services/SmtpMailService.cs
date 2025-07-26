using System;
using System.Linq;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    public class SmtpMailService : IMailService
    {
        private readonly ILogger<SmtpMailService> _logger;
        private readonly SimplySmtpSettings _simplySmtpSettings;

        public SmtpMailService(
            ILogger<SmtpMailService> logger,
            IOptions<SimplySmtpSettings> simplySmtpSettings
            )
        {
            _logger = logger;
            _simplySmtpSettings = simplySmtpSettings.Value;
        }

        /// <inheritdoc/>
        // public async Task SendRentalConfirmationEmail(RentalEntity rental, decimal totalPrice)
        // {
        //     var dto = new SendGridMailDto(
        //         templateId: _sendGridSettings.RentalConfirmationTemplateId,
        //         templateData: new
        //         {
        //             rentalId = rental.Id,
        //             paymentMethod = rental.PaymentMethod.ToString(),
        //             dealCoupon = rental.DealCoupon,
        //             fullName = rental.FullName,
        //             items = rental.Items
        //                 .Select(x => new
        //                 {
        //                     count = x.Count,
        //                     name = x.Product.Name
        //                 })
        //                 .ToArray(),
        //             startDate = rental.StartDate,
        //             endDate = rental.EndDate,
        //             totalPrice = totalPrice,
        //             cancellationLink = _sendGridSettings.RentalCancellationLink.Replace("{id}", rental.Id.ToString()),
        //             finishLink = _sendGridSettings.RentalFinishLink
        //                 .Replace("{id}", rental.Id.ToString())
        //                 .Replace("{phone}", rental.Phone), // this might be prone to errors if a phonenumber contains "+" and stuff
        //         },
        //         from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
        //         to: new EmailAddress(rental.EmailAddress, rental.FullName),
        //         replyTo: null
        //     );

        //     await SendEmail(dto);
        // }

        /// <inheritdoc/>
        public async Task SendRentalConfirmationEmail(RentalEntity rental, decimal totalPrice)
        {
            if (rental == null) throw new ArgumentNullException(nameof(rental)); var bodyHtml = $@"
                <p>Hej {rental.FullName}!</p>
                <br>
                <p>Du har hermed fra {rental.StartDate:dd/MM-yyyy} til {rental.EndDate:dd/MM-yyyy} booket:</p>
                {string.Join("<br>", rental.Items.Select(x => $"- {x.Count} {x.Product.Name}"))}
                <br>
                {rental.PaymentMethod switch
            {
                PaymentMethodEnum.DealCoupon => $"<p>Du har valgt Deal-bevis som betalingsmetode med kuponen: {rental.DealCoupon}</p>",
                PaymentMethodEnum.MobilePay => $"<p>Du har valgt MobilePay som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris {totalPrice} kr.</p><p>Ved betaling bedes du angive referencenummeret i kommentarfeltet: {rental.Id}</p>",
                PaymentMethodEnum.BankTransfer => $"<p>Du har valgt bankoverførsel som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris {totalPrice} kr.</p><p>Ved overførsel bedes du angive referencenummeret i kommentarfeltet: {rental.Id}</p><p>Kontooplysninger: Nordea, reg 1971, konto 6279257276.</p>",
                _ => ""
            }}
                <br>
                <p>Du kan annullere din booking <a href='{_simplySmtpSettings.RentalCancellationLink.Replace("{id}", rental.Id.ToString())}' target='_blank'>her</a> eller ved at kontakte os.</p>
                <p>Når du er færdig med at bruge udstyret bedes du melde det ledigt <a href='{_simplySmtpSettings.RentalFinishLink.Replace("{id}", rental.Id.ToString()).Replace("{phone}", rental.Phone)}' target='_blank'>her</a>.</p>
                <p>Du er velkommen til at ringe til Ken på 23244171 for yderligere info et par dage før du skal på vandet.</p>
                <p>Mvh Århus Vandsportscenter</p>
            ";

            await SendEmailUsingSmtp("Din booking hos Århus Vandsportscenter", bodyHtml, rental.EmailAddress);
            _logger.LogInformation("Sent rental confirmation email to {EmailAddress} for rental {RentalId}", rental.EmailAddress, rental.Id);
        }

        /// <inheritdoc/>
        public async Task SendRentalFinishedEmail(RentalEntity rental)
        {
            throw new NotImplementedException("SendRentalFinishedEmail is not implemented for SmtpMailService.");
        }

        /// <inheritdoc/>
        public async Task SendRentalCanceledEmail(RentalEntity rental)
        {
            throw new NotImplementedException("SendRentalCanceledEmail is not implemented for SmtpMailService.");
        }

        /// <inheritdoc/>
        public async Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken)
        {
            throw new NotImplementedException("SendResetPasswordEmail is not implemented for SmtpMailService.");
        }

        /// <inheritdoc/>
        public async Task SendContactEmail(string fromEmail, string fullName, string comment)
        {
            throw new NotImplementedException("SendContactEmail is not implemented for SmtpMailService.");
        }

        // /// <inheritdoc/>
        // public async Task SendRentalCanceledEmail(RentalEntity rental)
        // {
        //     var dto = new SendGridMailDto(
        //         templateId: _sendGridSettings.RentalCancellationTemplateId,
        //         templateData: new
        //         {
        //             rentalId = rental.Id,
        //             fullName = rental.FullName,
        //             startDate = rental.StartDate,
        //             endDate = rental.EndDate
        //         },
        //         from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
        //         to: new EmailAddress(rental.EmailAddress, rental.FullName),
        //         replyTo: null
        //     );

        //     await SendEmail(dto);
        // }

        // public async Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken)
        // {
        //     var dto = new SendGridMailDto(
        //         templateId: _sendGridSettings.ResetPasswordTemplateId,
        //         templateData: new
        //         {
        //             fullName = fullName,
        //             resetLink = _sendGridSettings.ResetPasswordLink.Replace("{passwordToken}", resetPasswordToken.ToString())
        //         },
        //         from: new EmailAddress(_sendGridSettings.SendFromEmail, _sendGridSettings.SendFromName),
        //         to: new EmailAddress(email, fullName),
        //         replyTo: null
        //     );

        //     await SendEmail(dto);
        // }

        // public async Task SendContactEmail(string fromEmail, string fullName, string comment)
        // {
        //     var dto = new SendGridMailDto(
        //         templateId: _sendGridSettings.ContactTemplateId,
        //         templateData: new
        //         {
        //             fullName = fullName,
        //             email = fromEmail,
        //             comment = comment
        //         },
        //         from: new EmailAddress(_sendGridSettings.SendFromEmail, fullName),
        //         to: new EmailAddress(_sendGridSettings.ContactMailToEmail, _sendGridSettings.ContactMailToName),
        //         replyTo: new EmailAddress(fromEmail, fullName)
        //     );

        //     await SendEmail(dto);
        // }

        // rewrite SendEmail to use SMTP instead of SendGrid
        protected virtual async Task SendEmailUsingSmtp(string subject, string bodyHtml, string toEmail)
        {

            using var smtpClient = new System.Net.Mail.SmtpClient(_simplySmtpSettings.Host, _simplySmtpSettings.Port)
            {
                Credentials = new System.Net.NetworkCredential(_simplySmtpSettings.Email, _simplySmtpSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_simplySmtpSettings.SendFromEmail, _simplySmtpSettings.SendFromName),
                Subject = subject,
                To = { new System.Net.Mail.MailAddress(toEmail) },
                ReplyToList = { new System.Net.Mail.MailAddress(_simplySmtpSettings.SendFromEmail) },
                Body = bodyHtml,
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        // protected virtual async Task SendEmail(SendGridMailDto mailDto)
        // {
        //     var msg = new SendGridMessage();
        //     msg.AddTo(mailDto.To.Email, mailDto.To.Name);
        //     if (mailDto.ReplyTo != null)
        //         msg.SetReplyTo(new EmailAddress(mailDto.ReplyTo.Email, mailDto.ReplyTo.Name));
        //     msg.SetFrom(mailDto.From.Email, mailDto.From.Name);
        //     msg.SetTemplateId(mailDto.TemplateId);
        //     msg.SetTemplateData(mailDto.TemplateData);

        //     var response = await _sendGridClient.SendEmailAsync(msg);
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var content = await response.Body.ReadAsStringAsync();
        //         throw new Exception($"SendGrid invoked sending to with template {msg.TemplateId}, response {content} {response.Body}");
        //     }
        // }
    }
}