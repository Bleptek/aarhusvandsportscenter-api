using System;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Aarhusvandsportscenter.Api.Tests.Domain.Services
{
    public class SmtpMailServiceTests
    {
        private static SimplySmtpSettings BuildSettings() => new SimplySmtpSettings
        {
            Email = "info@aarhusvandsportscenter.dk",
            Password = "secret",
            Host = "websmtp.simply.com",
            Port = 587,
            ContactMailToName = "Aarhus Vandsportscenter",
            ContactMailToEmail = "mail@thomasw.dk",
            SendFromName = "Aarhus Vandsportscenter",
            SendFromEmail = "mail@aarhusvandsportscenter.dk",
            RentalCancellationLink = "http://localhost:3000/booking/aflys/{id}",
            RentalFinishLink = "http://localhost:3000/booking/done/{id}/{phone}",
            ResetPasswordLink = "http://localhost:3000/opdater-adganskode?passwordToken={passwordToken}"
        };

        private static STUB_Mailservice CreateService() =>
            new STUB_Mailservice(NullLogger<SmtpMailService>.Instance, Options.Create(BuildSettings()));

        private static RentalEntity CreateRental(PaymentMethodEnum paymentMethod = PaymentMethodEnum.MobilePay)
        {
            var rental = new RentalEntity("Thomas Willumsen", "+45 12 12 12 12", "someemail@example.com", new DateTime(2024, 3, 5), new DateTime(2024, 3, 9))
            {
                Id = 42,
                PaymentMethod = paymentMethod,
                DealCoupon = "DEAL123",
                Items = new List<RentalItemEntity>
                {
                    new RentalItemEntity { Count = 2, Product = new RentalProductEntity("Kajak", "Kajakker", 5) }
                }
            };
            return rental;
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalConfirmationEmail_MobilePay_SendsExpectedEmail()
        {
            var service = CreateService();
            var rental = CreateRental(PaymentMethodEnum.MobilePay);

            await service.SendRentalConfirmationEmail(rental, 1500m);

            var email = Assert.Single(service.SentEmails);
            Assert.Equal("Booking bekræftelse", email.Subject);
            Assert.Equal(rental.EmailAddress, email.ToEmail);
            Assert.Contains("Du har booket udstyr hos Aarhus Vandsportscenter", email.BodyHtml); // preheader
            Assert.Contains("Hej Thomas Willumsen !", email.BodyHtml);
            Assert.Contains("fra 5/3-2024 til 9/3-2024 booket", email.BodyHtml); // no leading zeroes
            Assert.Contains("<p>- 2 Kajak</p>", email.BodyHtml);
            Assert.Contains("Du har valgt MobilePay som betalingsmetode.", email.BodyHtml);
            Assert.Contains("den samlede pris 1500kr.", email.BodyHtml); // no space before "kr."
            Assert.Contains("referencenummeret i kommentarfeltet: 42", email.BodyHtml);
            Assert.Contains("http://localhost:3000/booking/aflys/42", email.BodyHtml);
            Assert.Contains("http://localhost:3000/booking/done/42/+45 12 12 12 12", email.BodyHtml);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalConfirmationEmail_DealCoupon_IncludesCouponText()
        {
            var service = CreateService();
            var rental = CreateRental(PaymentMethodEnum.DealCoupon);

            await service.SendRentalConfirmationEmail(rental, 0m);

            var email = Assert.Single(service.SentEmails);
            Assert.Contains("Du har valgt Deal-bevis som betalingsmetode med kuponen: DEAL123", email.BodyHtml);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalConfirmationEmail_BankTransfer_IncludesBankDetails()
        {
            var service = CreateService();
            var rental = CreateRental(PaymentMethodEnum.BankTransfer);

            await service.SendRentalConfirmationEmail(rental, 1500m);

            var email = Assert.Single(service.SentEmails);
            Assert.Contains("Du har valgt bankoverførsel som betalingsmetode.", email.BodyHtml);
            Assert.Contains("den samlede pris 1500kr.", email.BodyHtml);
            Assert.Contains("Nordea, reg 1971, konto 6279257276.", email.BodyHtml);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalConfirmationEmail_NullRental_Throws()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendRentalConfirmationEmail(null, 0m));
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalCanceledEmail_SendsExpectedEmail()
        {
            var service = CreateService();
            var rental = CreateRental();

            await service.SendRentalCanceledEmail(rental);

            var email = Assert.Single(service.SentEmails);
            Assert.Equal("Booking annulleret", email.Subject);
            Assert.Equal(rental.EmailAddress, email.ToEmail);
            Assert.Contains("Din booking hos Aarhus Vandsportscenter er aflyst", email.BodyHtml); // preheader
            Assert.Contains("Hej Thomas Willumsen !", email.BodyHtml);
            Assert.Contains("Din booking med referencenummer 42 i perioden 5/3-2024 til 9/3-2024 er hermed aflyst.", email.BodyHtml);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendRentalCanceledEmail_NullRental_Throws()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendRentalCanceledEmail(null));
        }

        [Fact]
        public async System.Threading.Tasks.Task SendResetPasswordEmail_SendsExpectedEmail()
        {
            var service = CreateService();
            var token = Guid.NewGuid();

            await service.SendResetPasswordEmail("someone@example.com", "Thomas Willumsen", token);

            var email = Assert.Single(service.SentEmails);
            Assert.Equal("Ny adgangskode", email.Subject);
            Assert.Equal("someone@example.com", email.ToEmail);
            Assert.Contains("Vælg ny adgangskode", email.BodyHtml); // preheader
            Assert.Contains("Hej Thomas Willumsen", email.BodyHtml);
            Assert.Contains($"http://localhost:3000/opdater-adganskode?passwordToken={token}", email.BodyHtml);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendContactEmail_SendsToContactMailboxWithReplyToVisitor()
        {
            var service = CreateService();

            await service.SendContactEmail("visitor@example.com", "Jane Doe", "Hvornår kan jeg leje en kajak?");

            var email = Assert.Single(service.SentEmails);
            Assert.Equal("Aarhus Vandsportscenter kontaktbesked", email.Subject);
            Assert.Equal("mail@thomasw.dk", email.ToEmail); // ContactMailToEmail
            Assert.Equal("Jane Doe", email.FromName);
            Assert.Equal("visitor@example.com", email.ReplyToEmail);
            Assert.Equal("Jane Doe", email.ReplyToName);
            Assert.Contains("Fra Jane Doe", email.BodyHtml); // preheader
            Assert.Contains("Navn: Jane Doe", email.BodyHtml);
            Assert.Contains("Mail: visitor@example.com", email.BodyHtml);
            Assert.Contains("Kommentar: Hvornår kan jeg leje en kajak?", email.BodyHtml);
        }
    }
}
