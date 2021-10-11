using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class RentalEntity : BaseEntity
    {
        public RentalEntity()
        {
        }

        public RentalEntity(string fullName, string phone, string emailAddress, DateTime startDate, DateTime endDate)
        {
            FullName = fullName;
            Phone = phone;
            EmailAddress = emailAddress;
            StartDate = startDate;
            EndDate = endDate;
            Items = new List<RentalItemEntity>();
        }

        [Required]
        public string FullName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string AdminComment { get; set; }
        public bool Done { get; set; } = false;
        [Required]
        public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.MobilePay;
        public string DealCoupon { get; set; }
        public string DealSite { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public RentalCategoryEntity Category { get; set; }

        public IEnumerable<RentalItemEntity> Items { get; set; }
    }
}
