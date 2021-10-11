using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers.Rentals
{
    public class UpdateRentalRequest
    {
        public string Comment { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
    }
}