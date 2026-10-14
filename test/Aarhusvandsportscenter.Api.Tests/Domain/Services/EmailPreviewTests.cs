using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Tests.TestUtils.Stubs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Aarhusvandsportscenter.Api.Tests.Domain.Services
{
    /// <summary>
    /// Writes every email to an .html file so the design can be checked in a browser without deploying.
    /// Run with: dotnet test --filter EmailPreviewTests --logger "console;verbosity=detailed"
    /// Output folder: %TEMP%\aarhusvandsportscenter-email-previews (or the EMAIL_PREVIEW_DIR environment variable).
    /// </summary>
    public class EmailPreviewTests
    {
        [Fact]
        public async Task GenerateEmailPreviews()
        {
            var outputDir = Environment.GetEnvironmentVariable("EMAIL_PREVIEW_DIR")
                ?? Path.Combine(Path.GetTempPath(), "aarhusvandsportscenter-email-previews");
            Directory.CreateDirectory(outputDir);

            var service = new STUB_Mailservice(NullLogger<SmtpMailService>.Instance, Options.Create(new SimplySmtpSettings
            {
                ContactMailToEmail = "info@aarhusvandsportscenter.dk",
                SendFromName = "Aarhus Vandsportscenter",
                SendFromEmail = "mail@aarhusvandsportscenter.dk",
                RentalCancellationLink = "https://aarhusvandsportscenter.dk/booking/aflys/{id}",
                RentalFinishLink = "https://aarhusvandsportscenter.dk/booking/done/{id}/{phone}",
                ResetPasswordLink = "https://aarhusvandsportscenter.dk/opdater-adganskode?passwordToken={passwordToken}"
            }));

            RentalEntity CreateRental(PaymentMethodEnum paymentMethod) =>
                new RentalEntity("Thomas Willumsen", "12121212", "someemail@example.com", new DateTime(2024, 3, 5), new DateTime(2024, 3, 9))
                {
                    Id = 42,
                    PaymentMethod = paymentMethod,
                    DealCoupon = "DEAL123",
                    Items = new List<RentalItemEntity>
                    {
                        new RentalItemEntity { Count = 2, Product = new RentalProductEntity("Kajak", "Kajakker", 5) },
                        new RentalItemEntity { Count = 1, Product = new RentalProductEntity("Paddle board", "Paddle boards", 5) }
                    }
                };

            await service.SendRentalConfirmationEmail(CreateRental(PaymentMethodEnum.MobilePay), 1500m);
            await service.SendRentalConfirmationEmail(CreateRental(PaymentMethodEnum.BankTransfer), 1500m);
            await service.SendRentalConfirmationEmail(CreateRental(PaymentMethodEnum.DealCoupon), 0m);
            await service.SendRentalCanceledEmail(CreateRental(PaymentMethodEnum.MobilePay));
            await service.SendResetPasswordEmail("someone@example.com", "Thomas Willumsen", Guid.NewGuid());
            await service.SendContactEmail("visitor@example.com", "Jane Doe", "Hvornår kan jeg leje en kajak?\nOg hvad koster det?");

            var names = new[] { "1-booking-mobilepay", "2-booking-bankoverforsel", "3-booking-deal", "4-booking-annulleret", "5-nulstil-adgangskode", "6-kontaktbesked" };
            for (var i = 0; i < names.Length; i++)
            {
                File.WriteAllText(Path.Combine(outputDir, names[i] + ".html"), service.SentEmails[i].BodyHtml, new System.Text.UTF8Encoding(false));
            }

            Console.WriteLine($"Email previews written to: {outputDir}");
            Assert.Equal(names.Length, service.SentEmails.Count);
        }
    }
}
