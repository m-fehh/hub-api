using System.Text.RegularExpressions;

namespace Hub.Application.Validators
{
    /// <summary>
    /// Classe responsável por verificar a validade de um telefone
    /// </summary>
    public class PhoneValidator
    {
        public bool IsPhoneValid(PhoneParameter phone)
        {
            if (phone == null || string.IsNullOrEmpty(phone.Number) || string.IsNullOrEmpty(phone.AreaCode))
            {
                return false;
            }

            return IsPhoneNumberValid(phone)
                && IsPhoneAreaCodeValid(phone);
        }

        public string IsPhoneNumberSequenceChecker(PhoneParameter phone)
        {
            string acceptedPattern = @"(?!(\d)\1{8})\d{" + phone.Number.Length + "}";

            // verifica se o campo é sequencia repetida do mesmo número ex.: 9999-9999 ou 99999-9999
            if (!Regex.IsMatch(phone.Number, acceptedPattern, RegexOptions.IgnoreCase))
            {
                Random randNum = new Random();
                string number = "99";

                for (int i = 0; i <= 6; i++)
                    number += randNum.Next().ToString().Substring(0, 1);

                return number;
            }
            else
                return phone.Number;
        }

        public bool IsPhoneNumberValid(PhoneParameter phone)
        {
            string acceptedPattern = @"^([0-9]{8,9}|[0-9]{4,5}-[0-9]{4})$";

            // verifica se o campo está no padrão 99999999 ou 999999999
            if (!Regex.IsMatch(phone.Number, acceptedPattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            return true;
        }

        public bool IsPhoneAreaCodeValid(PhoneParameter phone)
        {
            var acceptedPattern = @"^([0-9]{2}|\([0-9]{2}\))$";

            // verifica se o campo está no padrão 99
            if (!Regex.IsMatch(phone.AreaCode, acceptedPattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            return true;
        }
    }

    public class PhoneParameter
    {
        public PhoneParameter(string countryCode, string areaCode, string number)
        {
            PhoneCountryCode = countryCode;
            Number = number;
            AreaCode = areaCode;
        }

        public string Number { get; }
        public string AreaCode { get; }
        public string PhoneCountryCode { get; }
    }
}
