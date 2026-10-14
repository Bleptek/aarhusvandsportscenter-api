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

            var itemsHtml = string.Join(Environment.NewLine, rental.Items.Select(x => $"<li style=\"margin:0 0 4px 0;\">{x.Count} {Encode(x.Product.Name)}</li>"));
            var totalPriceText = FormatPrice(totalPrice);
            var paymentMethodHtml = rental.PaymentMethod switch
            {
                PaymentMethodEnum.DealCoupon => $"<p>Du har valgt Deal-bevis som betalingsmetode med kuponen: <strong>{Encode(rental.DealCoupon)}</strong></p>",
                PaymentMethodEnum.MobilePay => $"<p>Du har valgt MobilePay som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris <strong>{totalPriceText}kr.</strong></p><p>Ved betaling bedes du angive referencenummeret i kommentarfeltet: <strong>{rental.Id}</strong></p>",
                PaymentMethodEnum.BankTransfer => $"<p>Du har valgt bankoverførsel som betalingsmetode.</p><p>Medmindre andet er aftalt er den samlede pris <strong>{totalPriceText}kr.</strong></p><p>Ved overførsel bedes du angive referencenummeret i kommentarfeltet: <strong>{rental.Id}</strong></p><p>Kontooplysninger: Nordea, reg 1971, konto 6279257276.</p>",
                _ => ""
            };
            var paymentBoxHtml = string.IsNullOrEmpty(paymentMethodHtml) ? "" : Callout(paymentMethodHtml);

            var bodyHtml = BuildEmail("Du har booket udstyr hos Aarhus Vandsportscenter", "Booking bekræftelse", $@"
                <p>Hej {Encode(rental.FullName)} !</p>
                <p>Du har hermed fra <strong>{FormatDate(rental.StartDate)}</strong> til <strong>{FormatDate(rental.EndDate)}</strong> booket:</p>
                <ul style=""margin:0 0 20px 0;padding:0 0 0 20px;"">
                {itemsHtml}
                </ul>
                {paymentBoxHtml}
                <p>Du kan annullere din booking <a href='{_simplySmtpSettings.RentalCancellationLink.Replace("{id}", rental.Id.ToString())}' target='_blank' style=""{LinkStyle}"">her</a> eller ved at kontakte os.</p>
                <p>Når du er færdig med at bruge udstyret bedes du melde det ledigt <a href='{_simplySmtpSettings.RentalFinishLink.Replace("{id}", rental.Id.ToString()).Replace("{phone}", rental.Phone)}' target='_blank' style=""{LinkStyle}"">her</a>.</p>
                <p>Du er velkommen til at ringe til Ken på 23244171 for yderligere info et par dage før du skal på vandet.</p>
                <p>Mvh<br>Århus Vandsportscenter</p>
            ");

            await SendEmailUsingSmtp("Booking bekræftelse", bodyHtml, rental.EmailAddress);
            _logger.LogInformation("Sent rental confirmation email to {EmailAddress} for rental {RentalId}", rental.EmailAddress, rental.Id);
        }

        /// <inheritdoc/>
        public async Task SendRentalCanceledEmail(RentalEntity rental)
        {
            if (rental == null) throw new ArgumentNullException(nameof(rental));

            var bodyHtml = BuildEmail("Din booking hos Aarhus Vandsportscenter er aflyst", "Booking annulleret", $@"
                <p>Hej {Encode(rental.FullName)} !</p>
                <p>Din booking med referencenummer {rental.Id} i perioden {FormatDate(rental.StartDate)} til {FormatDate(rental.EndDate)} er hermed aflyst.</p>
                <p>Mvh<br>Århus Vandsportscenter</p>
            ");

            await SendEmailUsingSmtp("Booking annulleret", bodyHtml, rental.EmailAddress);
            _logger.LogInformation("Sent rental cancellation email to {EmailAddress} for rental {RentalId}", rental.EmailAddress, rental.Id);
        }

        /// <inheritdoc/>
        public async Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email must be provided", nameof(email));

            var resetLink = _simplySmtpSettings.ResetPasswordLink.Replace("{passwordToken}", resetPasswordToken.ToString());
            var bodyHtml = BuildEmail("Vælg ny adgangskode", "Ny adgangskode", $@"
                <p>Hej {Encode(fullName)}</p>
                <p>Vi har modtaget en anmodning om at nulstille din adgangskode. Klik på knappen herunder for at vælge en ny.</p>
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin:24px 0;"">
                  <tr>
                    <td align=""center"" bgcolor=""{Accent}"" style=""border-radius:6px;"">
                      <a href='{resetLink}' target='_blank' style=""display:inline-block;padding:12px 28px;font-size:16px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:6px;"">Vælg ny adgangskode</a>
                    </td>
                  </tr>
                </table>
                <p style=""font-size:13px;color:#8d8d8d;"">Virker knappen ikke, kan du kopiere dette link ind i din browser:<br><a href='{resetLink}' target='_blank' style=""{LinkStyle}word-break:break-all;"">{resetLink}</a></p>
            ");

            await SendEmailUsingSmtp("Ny adgangskode", bodyHtml, email);
            _logger.LogInformation("Sent reset password email to {EmailAddress}", email);
        }

        /// <inheritdoc/>
        public async Task SendContactEmail(string fromEmail, string fullName, string comment)
        {
            if (string.IsNullOrWhiteSpace(fromEmail)) throw new ArgumentException("FromEmail must be provided", nameof(fromEmail));

            var commentHtml = Encode(comment).Replace("\n", "<br>");
            var bodyHtml = BuildEmail($"Fra {Encode(fullName)}", "Kontaktbesked", $@"
                <p>Navn: <strong>{Encode(fullName)}</strong></p>
                <p>Mail: <a href='mailto:{Encode(fromEmail)}' style=""{LinkStyle}"">{Encode(fromEmail)}</a></p>
                {Callout($"<p>Kommentar: {commentHtml}</p>")}
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

        // Colors match the website theme
        private const string Dark = "#191516";
        private const string Accent = "#00b7e0";
        private const string AccentDark = "#0096b8";
        private const string PageBackground = "#f4f4f6";
        private const string LinkStyle = "color:#0096b8;font-weight:bold;";

        private static string Encode(string value) =>
            (value ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");

        /// <summary>
        /// Highlighted box with an accent border, used for payment info and visitor comments.
        /// </summary>
        private static string Callout(string innerHtml) =>
            $@"<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin:0 0 20px 0;"">
                  <tr><td style=""background-color:{PageBackground};border-left:4px solid {Accent};padding:16px 20px;border-radius:4px;"">{innerHtml}</td></tr>
                </table>";

        /// <summary>
        /// Wraps <paramref name="contentHtml"/> in the branded email layout (header, padded card, footer). It also
        /// adds a hidden preheader, which most mail clients show as the inbox preview text instead of the start of the visible body.
        /// Styling is inline/table based for compatibility with mail clients.
        /// </summary>
        private static string BuildEmail(string preheaderText, string title, string contentHtml) =>
            $@"<!DOCTYPE html>
<html lang=""da"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>{Encode(title)}</title>
<style>
  p {{ margin: 0 0 16px 0; }}
</style>
</head>
<body style=""margin:0;padding:0;background-color:{PageBackground};"">
<div style=""display:none;max-height:0px;overflow:hidden;"">{preheaderText}</div>
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:{PageBackground};"">
  <tr>
    <td align=""center"" style=""padding:32px 16px;"">
      <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:100%;max-width:600px;background-color:#ffffff;border-radius:8px;overflow:hidden;font-family:'Work Sans','Helvetica Neue',Helvetica,Arial,sans-serif;"">
        <tr>
          <td style=""background-color:{Dark};padding:28px 40px;border-bottom:4px solid {Accent};"">
            <div style=""font-size:13px;letter-spacing:2px;text-transform:uppercase;color:{Accent};font-weight:bold;"">Aarhus Vandsportscenter</div>
            <div style=""font-size:24px;line-height:30px;color:#ffffff;font-weight:bold;padding-top:6px;"">{Encode(title)}</div>
          </td>
        </tr>
        <tr>
          <td style=""padding:36px 40px 20px 40px;font-size:16px;line-height:24px;color:{Dark};"">
            {contentHtml}
          </td>
        </tr>
        <tr>
          <td style=""padding:20px 40px 28px 40px;border-top:1px solid #eaedf2;font-size:12px;line-height:18px;color:#8d8d8d;"">
            Aarhus Vandsportscenter &middot; <a href=""https://aarhusvandsportscenter.dk"" target=""_blank"" style=""color:{AccentDark};text-decoration:none;"">aarhusvandsportscenter.dk</a>
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</body>
</html>";

        /// <summary>
        /// Formats a date as "D/M-YYYY" (day and month without leading zeros), matching the old SendGrid templates.
        /// </summary>
        private static string FormatDate(DateTime date) => $"{date.Day}/{date.Month}-{date.Year}";

        // Max 2 decimals; whole amounts are shown without decimals (e.g. "1500"), others as "1500,50"
        private static string FormatPrice(decimal price)
        {
            var rounded = Math.Round(price, 2, MidpointRounding.AwayFromZero);
            var format = rounded == Math.Truncate(rounded) ? "0" : "0.00";
            return rounded.ToString(format, System.Globalization.CultureInfo.GetCultureInfo("da-DK"));
        }

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
