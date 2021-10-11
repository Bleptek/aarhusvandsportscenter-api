using System;
using System.Collections.Generic;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class RentalResponse
    {
        public RentalResponse() { }
        public RentalResponse(RentalEntity model)
        {
            FullName = model.FullName;
            Phone = model.Phone;
            EmailAddress = model.EmailAddress;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            AdminComment = model.AdminComment;
            Done = model.Done;
            Id = model.Id;
            PaymentMethod = model.PaymentMethod;
            DealCoupon = model.DealCoupon;
            DealSite = model.DealSite;
            CategoryId = model.Category.Id;
            CategoryName = model.Category.Name;
            Items = model.Items.Select(x => new RentaltemCompactResponse(x));
        }

        public string FullName { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; private set; }
        public string AdminComment { get; set; }
        public bool Done { get; set; } = false;
        public int Id { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string DealCoupon { get; set; }
        public string DealSite { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public IEnumerable<RentaltemCompactResponse> Items { get; set; }
    }
}