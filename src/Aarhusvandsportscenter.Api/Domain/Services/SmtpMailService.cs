using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    /// <summary>
    /// Sends mail through Simply's SMTP relay. Replaces the previous SendGrid dynamic-template based
    /// implementation; the 4 emails below are hand-written HTML equivalents of the old SendGrid templates.
    /// </summary>
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
        public async Task SendRentalConfirmationEmail(RentalEntity rental, decimal totalPrice)
        {
            if (rental == null) throw new ArgumentNullException(nameof(rental));

            var itemsHtml = string.Join(Environment.NewLine, rental.Items.Select(x => $"<p>- {x.Count} {x.Product.Name}</p>"));
            var paymentMethodHtml = rental.PaymentMethod switch
            {
                PaymentMethodEnum.DealCoupon => $"<p>Du har valgt Deal-bevis som betalingsmetode med kuponen: {rental.DealCoupon}</p>",
                PaymentMethodEnum.MobilePay => $"<p>Du har valgt MobilePay som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris {totalPrice}kr.</p><p>Ved betaling bedes du angive referencenummeret i kommentarfeltet: {rental.Id}</p>",
                PaymentMethodEnum.BankTransfer => $"<p>Du har valgt bankoverførsel som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris {totalPrice}kr.</p><p>Ved overførsel bedes du angive referencenummeret i kommentarfeltet: {rental.Id}</p><p>Kontooplysninger: Nordea, reg 1971, konto 6279257276.</p>",
                _ => ""
            };

            var bodyHtml = WrapWithPreheader("Du har booket udstyr hos Aarhus Vandsportscenter", $@"
                <p>Hej {rental.FullName} !</p>
                <br>
                <p>Du har hermed fra {FormatDate(rental.StartDate)} til {FormatDate(rental.EndDate)} booket:</p>
                {itemsHtml}
                <br>
                {paymentMethodHtml}
                <br>
                <p>Du kan annullere din booking <a href='{_simplySmtpSettings.RentalCancellationLink.Replace("{id}", rental.Id.ToString())}' target='_blank'>her</a> eller ved at kontakte os.</p>
                <p>Når du er færdig med at bruge udstyret bedes du melde det ledigt <a href='{_simplySmtpSettings.RentalFinishLink.Replace("{id}", rental.Id.ToString()).Replace("{phone}", rental.Phone)}' target='_blank'>her</a>.</p>
                <p>Du er velkommen til at ringe til Ken på 23244171 for yderligere info et par dage før du skal på vandet.</p>
                <p>Mvh Århus Vandsportscenter</p>
            ");

            await SendEmailUsingSmtp("Booking bekræftelse", bodyHtml, rental.EmailAddress);
            _logger.LogInformation("Sent rental confirmation email to {EmailAddress} for rental {RentalId}", rental.EmailAddress, rental.Id);
        }

        /// <inheritdoc/>
        public async Task SendRentalCanceledEmail(RentalEntity rental)
        {
            if (rental == null) throw new ArgumentNullException(nameof(rental));

            var bodyHtml = WrapWithPreheader("Din booking hos Aarhus Vandsportscenter er aflyst", $@"
                <p>Hej {rental.FullName} !</p>
                <br>
                <p>Din booking med referencenummer {rental.Id} i perioden {FormatDate(rental.StartDate)} til {FormatDate(rental.EndDate)} er hermed aflyst.</p>
                <br>
                <p>Mvh Århus Vandsportscenter</p>
            ");

            await SendEmailUsingSmtp("Booking annulleret", bodyHtml, rental.EmailAddress);
            _logger.LogInformation("Sent rental cancellation email to {EmailAddress} for rental {RentalId}", rental.EmailAddress, rental.Id);
        }

        /// <inheritdoc/>
        public async Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email must be provided", nameof(email));

            var resetLink = _simplySmtpSettings.ResetPasswordLink.Replace("{passwordToken}", resetPasswordToken.ToString());
            var bodyHtml = WrapWithPreheader("Vælg ny adgangskode", $@"
                <p>Hej {fullName}</p>
                <br>
                <p>Vælg ny adgangskode <a href='{resetLink}' target='_blank'>her</a></p>
            ");

            await SendEmailUsingSmtp("Ny adgangskode", bodyHtml, email);
            _logger.LogInformation("Sent reset password email to {EmailAddress}", email);
        }

        /// <inheritdoc/>
        public async Task SendContactEmail(string fromEmail, string fullName, string comment)
        {
            if (string.IsNullOrWhiteSpace(fromEmail)) throw new ArgumentException("FromEmail must be provided", nameof(fromEmail));

            var bodyHtml = WrapWithPreheader($"Fra {fullName}", $@"
                <p>Navn: {fullName}</p>
                <p>Mail: {fromEmail}</p>
                <p>Kommentar: {comment}</p>
            ");

            await SendEmailUsingSmtp(
                subject: "Aarhus Vandsportscenter kontaktbesked",
                bodyHtml: bodyHtml,
                toEmail: _simplySmtpSettings.ContactMailToEmail,
                fromName: fullName,
                replyToEmail: fromEmail,
                replyToName: fullName);
            _logger.LogInformation("Sent contact email from {FromEmail} to {ToEmail}", fromEmail, _simplySmtpSettings.ContactMailToEmail);
        }

        /// <summary>
        /// Wraps <paramref name="contentHtml"/> with a hidden preheader, which most mail clients show as the
        /// inbox preview text instead of the start of the visible body.
        /// </summary>
        private static string WrapWithPreheader(string preheaderText, string contentHtml) =>
            $@"<div style=""display:none;max-height:0px;overflow:hidden;"">{preheaderText}</div>
            {contentHtml}";

        /// <summary>
        /// Formats a date as "D/M-YYYY" (day and month without leading zeros), matching the old SendGrid templates.
        /// </summary>
        private static string FormatDate(DateTime date) => $"{date.Day}/{date.Month}-{date.Year}";

        protected virtual async Task SendEmailUsingSmtp(
            string subject,
            string bodyHtml,
            string toEmail,
            string fromName = null,
            string replyToEmail = null,
            string replyToName = null)
        {
            using var smtpClient = new SmtpClient(_simplySmtpSettings.Host, _simplySmtpSettings.Port)
            {
                Credentials = new NetworkCredential(_simplySmtpSettings.Email, _simplySmtpSettings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_simplySmtpSettings.SendFromEmail, fromName ?? _simplySmtpSettings.SendFromName, Encoding.UTF8),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = bodyHtml,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(toEmail));
            mailMessage.ReplyToList.Add(new MailAddress(
                replyToEmail ?? _simplySmtpSettings.SendFromEmail,
                replyToName ?? _simplySmtpSettings.SendFromName,
                Encoding.UTF8));

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
