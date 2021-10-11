using System;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    public interface IMailService
    {
        Task SendContactEmail(string fromEmail, string fullName, string comment);
        Task SendRentalCanceledEmail(RentalEntity rental);

        /// <summary>
        /// </summary>
        /// <param name="rental">Must have included items and items.product </param>
        /// <param name="totalPrice"></param>
        Task SendRentalConfirmationEmail(RentalEntity rental, decimal totalPrice);
        Task SendResetPasswordEmail(string email, string fullName, Guid resetPasswordToken);
    }
}