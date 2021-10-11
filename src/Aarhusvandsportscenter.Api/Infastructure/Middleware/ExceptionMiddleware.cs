using Microsoft.AspNetCore.Http;
using Aarhusvandsportscenter.Api.Domain.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Aarhusvandsportscenter.Api.Controllers;

namespace Aarhusvandsportscenter.Api.Infastructure.Middleware
{
    /// <summary>
    /// middleware to catch exceptions and format the api response.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var httpStatusCode = HttpStatusCode.InternalServerError;
            var errorCode = ErrorCodes.INTERNAL.Code;
            var errorMessage = ErrorCodes.INTERNAL.Message;

            if (exception is NotFoundException nfEx)
            {
                httpStatusCode = HttpStatusCode.NotFound;
                errorCode = nfEx.ErrorCode;
                errorMessage = nfEx.ErrorMessage;
            }
            else if (exception is ConflictException cEx)
            {
                httpStatusCode = HttpStatusCode.Conflict;
                errorCode = cEx.ErrorCode;
                errorMessage = cEx.ErrorMessage;
            }
            else if (exception is BadFormatException bfEx)
            {
                httpStatusCode = HttpStatusCode.BadRequest;
                errorCode = bfEx.ErrorCode;
                errorMessage = bfEx.ErrorMessage;
            }
            else if (exception is BusinessRuleException brEx)
            {
                httpStatusCode = HttpStatusCode.UnprocessableEntity;
                errorCode = brEx.ErrorCode;
                errorMessage = brEx.ErrorMessage;
            }

            var errorResponse = new ApiErrorResponse(errorCode, errorMessage);
            var serializedResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogWarning("ExceptionMiddleware {errorResponse}", serializedResponse);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)httpStatusCode;
            return context.Response.WriteAsync(serializedResponse, context.RequestAborted);
        }
    }
}
