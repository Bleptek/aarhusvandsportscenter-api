using System;

namespace Aarhusvandsportscenter.Api.Domain.Exceptions
{
    /// <summary>
    /// To be thrown when business logic fails due to a conflict.
    /// This exception type is caught by the global filter and formatted before sending responses to the client.
    /// </summary>
    public class NotFoundException : BusinessRuleException
    {
        public NotFoundException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}