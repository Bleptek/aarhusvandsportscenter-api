using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;

namespace Aarhusvandsportscenter.Api.Controllers.RentalCategories
{
    public class RentalCategoryResponse
    {
        public RentalCategoryResponse() { }
        public RentalCategoryResponse(RentalCategoryEntity model)
        {
            Id = model.Id;
            Name = model.Name;
            IsDefault = model.IsDefault;
            ColorCode = model.ColorCode;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
        public bool IsDefault { get; set; }
    }
}