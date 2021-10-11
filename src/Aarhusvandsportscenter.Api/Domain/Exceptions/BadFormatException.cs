namespace Aarhusvandsportscenter.Api.Domain.Exceptions
{
    /// <summary>
    /// To be thrown when client input is badly formatted.
    /// This exception type is caught by the global filter and formatted before sending responses to the client.
    /// </summary>
    public class BadFormatException : BusinessRuleException
    {
        public BadFormatException(string propertyName) : base(
            ErrorCodes.PROPERTY_BAD_FORMAT.Code,
            string.Format(ErrorCodes.PROPERTY_BAD_FORMAT.Message, propertyName))
        {
        }
    }
}