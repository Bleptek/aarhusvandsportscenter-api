using System;

namespace Aarhusvandsportscenter.Api.Domain.Exceptions
{
    /// <summary>
    /// To be thrown when business logic fails.
    /// This exception type is caught by the global filter and formatted before sending responses to the client.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(ErrorCode errorCode)
        {
            ErrorCode = errorCode.Code;
            ErrorMessage = errorCode.Message;
        }
        public BusinessRuleException(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}