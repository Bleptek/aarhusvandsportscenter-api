using Aarhusvandsportscenter.Api.Controllers;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;

namespace Aarhusvandsportscenter.Api.Infastructure.Middleware
{
    /// <summary>
    /// Catch invalid model states and format a proper response to the client
    /// </summary>
    public static class ModelStateValidationBehaviour
    {
        /// <summary>
        /// handle modelstate validation errors
        /// </summary>
        public static IMvcBuilder ConfigureApiBehaviorOptions(this IMvcBuilder builder) =>
            builder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiErrorResponse(
                        ErrorCodes.VALIDATION.Code,
                        ErrorCodes.VALIDATION.Message,
                        context.ModelState.Keys
                            .SelectMany(key => context.ModelState[key].Errors
                                .Select(x => new ModelValidationError(key, x.ErrorMessage)))
                            .ToArray());

                    var result = new BadRequestObjectResult(errorResponse);
                    result.ContentTypes.Add(MediaTypeNames.Application.Json);

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogWarning("ModelStateValidationError {errorResponse}", JsonSerializer.Serialize(errorResponse));

                    return result;
                };
                // necessary to disable the defualt ProblemDetails model in Swagger
                options.SuppressMapClientErrors = true;
            });

    }
}
