using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Controllers.System;
using Aarhusvandsportscenter.Api.Infastructure.OldDatabase;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using System.Collections.Generic;
using Aarhusvandsportscenter.Api.Infastructure.OldDatabase.Entities;
using System;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly LeschleyDbContext _leschleyDbContext;
        private readonly AppDbContext _appDbContext;
        private readonly IMailService _mailService;

        public SystemController(
            LeschleyDbContext leschleyDbContext,
            AppDbContext appDbContext,
            IMailService mailService)
        {
            _mailService = mailService;
            _leschleyDbContext = leschleyDbContext;
            _appDbContext = appDbContext;
            _mailService = mailService;
        }

        /// <summary>
        /// Send email to the aarhus vandsportscenter admin
        /// </summary>
        [HttpPost("sendContactEmail", Name = nameof(SendContactEmail))]
        public async Task<IActionResult> SendContactEmail([FromBody] SendContactEmailRequest body)
        {
            await _mailService.SendContactEmail(body.FromEmail, body.FullName, body.Comment);
            return NoContent();
        }

        /// <summary>
        /// </summary>
        [HttpPost("migration", Name = nameof(Migration))]
        public async Task<IActionResult> Migration()
        {
            var lejereOLD = await _leschleyDbContext.Booking_Lejere.ToListAsync();
            var categories = await _appDbContext.RentalCategories.ToListAsync();
            var products = await _appDbContext.RentalProducts.ToListAsync();
            var rentalsNEW = MapRentals(lejereOLD, categories, products);
            _appDbContext.Rentals.AddRange(rentalsNEW);
            _appDbContext.SaveChanges();
            
            return NoContent();
        }

        private static IEnumerable<RentalEntity> MapRentals(List<BookingLejer> lejereOLD, List<RentalCategoryEntity> categories, List<RentalProductEntity> products)
        {
            var lejereNEW = lejereOLD.Select(x => new RentalEntity
            {
                AdminComment = x.Kommentar,
                Category = MapCategory(x.RowColor, categories),
                CreatedDate = x.Dato,
                DealCoupon = MapDeal(x.Deal, x.DealSite),
                DealSite = MapDealSite(x.Deal, x.DealSite),
                Done = x.Done ?? false,
                EmailAddress = x.Mail,
                EndDate = x.Dato,
                FullName = x.Navn,
                Id = x.Id,
                Items = MapItems(x, products),
                LastModifiedDate = x.Dato,
                PaymentMethod = MapPaymentMethod(x.Deal, x.DealSite),
                Phone = x.Tlf,
                StartDate = x.Dato
            });

            return lejereNEW;
        }

        private static PaymentMethodEnum MapPaymentMethod(string dealColumn, string dealSite){
            if(dealColumn.StartsWith("cash", StringComparison.InvariantCultureIgnoreCase))
                return PaymentMethodEnum.Unknown;

            else if(dealColumn.StartsWith("DEAL:", StringComparison.InvariantCultureIgnoreCase))
                return PaymentMethodEnum.DealCoupon;

            else if(!string.IsNullOrEmpty(dealSite))
                return PaymentMethodEnum.DealCoupon;

            else if(dealColumn.Contains("mobilePay", StringComparison.InvariantCultureIgnoreCase))
                return PaymentMethodEnum.MobilePay;

            else if(dealColumn.StartsWith("bankTransfer", StringComparison.InvariantCultureIgnoreCase))
                return PaymentMethodEnum.BankTransfer;

            else if(dealColumn.Contains("deal", StringComparison.InvariantCultureIgnoreCase))
                return PaymentMethodEnum.DealCoupon;

            else if(dealColumn.Contains(" ; "))
                return PaymentMethodEnum.DealCoupon;

            else if(dealColumn != "0" && !string.IsNullOrEmpty(dealColumn))
                return PaymentMethodEnum.DealCoupon;

            return PaymentMethodEnum.Unknown;
        }

        private static string MapDeal(string dealColumn, string dealSite){
            var paymentMethod = MapPaymentMethod(dealColumn, dealSite);
            if(paymentMethod != PaymentMethodEnum.DealCoupon && paymentMethod != PaymentMethodEnum.Unknown)
                return null;

            if(dealColumn.StartsWith("DEAL:", StringComparison.InvariantCultureIgnoreCase)){
                var deal = string.Join("", dealColumn.Split(":").Skip(1));
                return deal;
            }

            if(dealColumn == "0" || dealColumn == "")
                return null;

            return dealColumn;
        }

        private static string MapDealSite(string dealColumn, string dealSite){
            if(!string.IsNullOrEmpty(dealSite))
                return dealSite;

            if(MapPaymentMethod(dealColumn, dealSite) != PaymentMethodEnum.DealCoupon)
                return null;

            return "Unknown";
        }

        private static RentalCategoryEntity MapCategory(string rowColor, List<RentalCategoryEntity> categoriesNEW){
            if(rowColor == null)
                return categoriesNEW.First(x => x.Id == 1);

            switch (rowColor)
            {
                case "brown": return categoriesNEW.First(x => x.Id == 28);
                case "orange": return categoriesNEW.First(x => x.Id == 29);
                case "light-blue": return categoriesNEW.First(x => x.Id == 31);
                case "yellow": return categoriesNEW.First(x => x.Id == 26);
                case "light-red": return categoriesNEW.First(x => x.Id == 4);
                case "green": return categoriesNEW.First(x => x.Id == 24);
                case "purple": return categoriesNEW.First(x => x.Id == 33);
                case "dark-green": return categoriesNEW.First(x => x.Id == 25);
                case "light-purple": return categoriesNEW.First(x => x.Id == 34);
                default: throw new Exception($"couldnt translate category {rowColor}");
            }
        }
        
        private static List<RentalItemEntity> MapItems(BookingLejer rentalOLD, List<RentalProductEntity> products){
            var result = new List<RentalItemEntity>();

            if(rentalOLD.Jolle > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.Jolle, Product = products.First(x => x.Id == 4)});

            if(rentalOLD.BTrailer > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.BTrailer, Product = products.First(x => x.Id == 7)});

            if(rentalOLD.Dbkajak > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.Dbkajak, Product = products.First(x => x.Id == 2)});

            if(rentalOLD.Kajak > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.Kajak, Product = products.First(x => x.Id == 1)});

            if(rentalOLD.Kano > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.Kano, Product = products.First(x => x.Id == 3)});

            if(rentalOLD.Paddle > 0)
                result.Add(new RentalItemEntity{ Count = rentalOLD.Paddle, Product = products.First(x => x.Id == 5)});

            // der var ingen trailere (6) i det gamle system. kun bådtrailere.
            return result;
        }
    }
}