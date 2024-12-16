using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Internationalization
{
    /// <summary>
    /// Provides methods for validating and checking phone numbers.
    /// </summary>
    public static class PhoneNumberValidator
    {
        /// <summary>
        /// Gets or sets the default country code for phone number validation.
        /// </summary>
        public static string DefaultCountryCode { get; set; }

        /// <summary>
        /// Extension method that validates the specified phone number based on the provided country code.
        /// </summary>
        /// <param name="phoneNumberToValidate">The phone number to validate.</param>
        /// <param name="countryCode">The country code to use for validation.</param>
        /// <returns>True if the phone number is valid; otherwise, false.</returns>
        public static bool ValidatePhoneNumber(this string phoneNumberToValidate, string countryCode)
        {
            return Validate(phoneNumberToValidate, countryCode);
        }

        /// <summary>
        /// Validates the specified phone number based on the provided country code.
        /// </summary>
        /// <param name="phoneNumberToValidate">The phone number to validate.</param>
        /// <param name="countryCode">The country code to use for validation.</param>
        /// <returns>True if the phone number is valid; otherwise, false.</returns>
        public static bool Validate(this string phoneNumberToValidate, string countryCode)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var phoneNumber = phoneNumberUtil.Parse(phoneNumberToValidate, countryCode);

            var isValid = phoneNumberUtil.IsValidNumber(phoneNumber);

            return isValid;
        }

        /// <summary>
        /// Checks if the specified phone number is a mobile number based on the provided country code.
        /// </summary>
        /// <param name="phoneNumberToCheck">The phone number to check.</param>
        /// <param name="countryCode">The country code to use for checking.</param>
        /// <returns>True if the phone number is a mobile number; otherwise, false.</returns>
        public static bool ValidateMobilePhoneNumber(this string phoneNumberToCheck, string countryCode)
        {
            return IsMobile(phoneNumberToCheck, countryCode);
        }

        /// <summary>
        /// Checks if the specified phone number is a mobile number based on the provided country code.
        /// </summary>
        /// <param name="phoneNumberToCheck">The phone number to check.</param>
        /// <param name="countryCode">The country code to use for checking.</param>
        /// <returns>True if the phone number is a mobile number; otherwise, false.</returns>
        public static bool IsMobile(string phoneNumberToCheck, string countryCode)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var phoneNumber = phoneNumberUtil.Parse(phoneNumberToCheck, countryCode);

            var isValid = phoneNumberUtil.IsValidNumber(phoneNumber);
            var numberType = phoneNumberUtil.GetNumberType(phoneNumber);

            return isValid && (numberType == PhoneNumberType.MOBILE || numberType == PhoneNumberType.FIXED_LINE_OR_MOBILE);
        }
        /// <summary>
        /// Get country code using phoneNumber.
        /// </summary>
        /// <param name="systemNumber">The phone number to check.</param>
        /// <returns>The Country Code (Ex: US)</returns>
        public static string GetCountryCodeForNumber(this string systemNumber)
        {
            try
            {
                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(systemNumber.StartsWith("+") ? systemNumber : $"+{systemNumber}", null);
                return phoneNumberUtil.GetRegionCodeForNumber(phoneNumber);
            }
            catch
            {

                return "";
            }
        }
    }
}
