using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class CreateRentalRequest : IValidatableObject
    {
        public string DealCoupon { get; set; }
        public string DealSite { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public IEnumerable<CreateRentalItemRequest> Items { get; set; }
        [Required]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public RentalEntity MapToEntity()
        {
            var result = new RentalEntity
            {
                DealCoupon = DealCoupon,
                DealSite = DealSite,
                EmailAddress = EmailAddress,
                FullName = FullName,
                Items = Items.Select(x => x.MapToEntity()).ToArray(),
                PaymentMethod = PaymentMethod,
                Phone = Phone,
                StartDate = StartDate,
                EndDate = EndDate
            };

            return result;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(EndDate < StartDate)
                yield return new ValidationResult("Enddate must be greater than or equal to startdate", new string[]{nameof(EndDate)});

            if(!string.IsNullOrEmpty(DealCoupon) && string.IsNullOrEmpty(DealSite))
                yield return new ValidationResult("If deal coupon is set, deal site must have a value as well", new string[]{nameof(DealSite)});
        }
    }
}