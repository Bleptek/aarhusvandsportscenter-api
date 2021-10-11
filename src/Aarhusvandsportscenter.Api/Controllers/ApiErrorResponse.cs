using System.ComponentModel.DataAnnotations;

namespace Aarhusvandsportscenter.Api.Controllers
{
    /// <summary>
    /// The typical error type that will be sent to the client when an error happens in the API
    /// </summary>
    public class ApiErrorResponse
    {
        public ApiErrorResponse(int errorCode, string errorMessage, ModelValidationError[] validationErrors = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            ValidationErrors = validationErrors ?? new ModelValidationError[] { };
        }

        [Required]
        public int ErrorCode { get; set; }

        [Required]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// will contain validation errors if any
        /// </summary>
        [Required]
        public ModelValidationError[] ValidationErrors { get; set; } = new ModelValidationError[] { };
    }

    public class ModelValidationError
    {
        public ModelValidationError(string field, string description)
        {
            Field = field;
            Description = description;
        }

        [Required]
        public string Field { get; set; }

        [Required]
        public string Description { get; set; }
    }
}