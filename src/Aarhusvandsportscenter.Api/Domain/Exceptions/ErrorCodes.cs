namespace Aarhusvandsportscenter.Api.Domain.Exceptions
{
    /// <summary>
    /// Error messages to use when business rules are broken.
    /// These should be encapsulated in an exception - for instance the BusinessRuleException.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary></summary>
        private static int errorRoot = 100000;

        // ==== GENERIC 0-10000 ============
        public static ErrorCode INTERNAL = new ErrorCode(errorRoot + 1000, "An unhandled error occured. Staff have been notified");
        public static ErrorCode VALIDATION = new ErrorCode(errorRoot + 2000, "Unable to process request. Validation of properties failed");
        public static ErrorCode UNAUTHORIZED = new ErrorCode(errorRoot + 3000, "Client is not authorized");
        public static ErrorCode FORBIDDEN = new ErrorCode(errorRoot + 3100, "Client is forbidden.");
        public static ErrorCode PROPERTY_BAD_FORMAT = new ErrorCode(errorRoot + 5000, "The property {0} is in an incorrect format");

        // ==== Account 10000-20000 ============
        #region Account
        public static class Account
        {
            private static int accountRoot = errorRoot + 10000;
            public static ErrorCode EMAIL_DOESNT_EXIST = new ErrorCode(accountRoot + 1, "The provided email doesnt exist");
            public static ErrorCode PASSWORD_NOT_CREATED = new ErrorCode(accountRoot + 2, "No login has been created for the provided email");
            public static ErrorCode PASSWORD_INVALID = new ErrorCode(accountRoot + 3, "The provided password is invalid");
            public static ErrorCode RESETPASSWORDTOKEN_INVALID = new ErrorCode(accountRoot + 4, "The reset password token is invalid. Might have already been used");
        }
        #endregion

        // ==== ContentPage 20000-30000 ============
        #region ContentPage
        public static class ContentPage
        {
            private static int contentPageRoot = errorRoot + 20000;
            public static ErrorCode KEY_DOESNT_EXIST = new ErrorCode(contentPageRoot + 1, "Content page key or section key doesnt exist");
            public static ErrorCode CONFLICT = new ErrorCode(contentPageRoot + 2, "Key already exists. Duplicates are not allowed");
        }
        #endregion

        // ==== Rental 30000-40000 ============
        #region Rental
        public static class Rentals
        {
            private static int rentalsRoot = errorRoot + 30000;
            public static ErrorCode RENTAL_DOESNT_EXIST = new ErrorCode(rentalsRoot + 1, "Rental does not exist");
            public static ErrorCode EXCEEDING_AVAILABLE_PRODUCT = new ErrorCode(rentalsRoot + 6, "The rental exceeds the amount of available product");
            public static ErrorCode RENTAL_FINISH_ILLEGAL_BEFORE_RENTAL_START = new ErrorCode(rentalsRoot + 7, "The rental cannot be finished before it has started. Consider cancelling it instead.");

            public static ErrorCode CATEGORY_CONFLICT = new ErrorCode(rentalsRoot + 50, "Name already exists. Duplicates are not allowed");
            public static ErrorCode CATEGORY_DOESNT_EXIST = new ErrorCode(rentalsRoot + 51, "Category doesnt exist");
            public static ErrorCode CATEGORY_IS_IN_USE = new ErrorCode(rentalsRoot + 52, "Category is in use by existing rentals and cannot be deleted");

            public static ErrorCode PRICE_DOESNT_EXIST = new ErrorCode(rentalsRoot + 100, "Rental product price does not exist");
            public static ErrorCode PRICE_CONFLICT = new ErrorCode(rentalsRoot + 101, "Quantity conflict. Duplicates are not allowed within the same product");
            public static ErrorCode PRICE_QUANTITY_1_MUST_EXIST = new ErrorCode(rentalsRoot + 102, "A price with a quantity of 1 must exist. It can not be modified or deleted");

            public static ErrorCode PRODUCT_DOESNT_EXIST = new ErrorCode(rentalsRoot + 150, "Rental product does not exist");
            public static ErrorCode PRODUCT_CONFLICT = new ErrorCode(rentalsRoot + 151, "Name already exists. Duplicates are not allowed");
            public static ErrorCode PRODUCT_IS_IN_USE = new ErrorCode(rentalsRoot + 152, "The product is in use by existing rentals and cannot be deleted");
        }
        #endregion
    }
}