using Hub.Infrastructure.Extensions;
using System.Text.RegularExpressions;

namespace Hub.Infrastructure.Internationalization.DocumentValidators
{
    /// <summary>
    /// Validator para documentos da Colômbia
    /// </summary>
    public static class ColombianDocumentValidator
    {
        /// <summary>
        /// Validator for the CC (Cédula de Ciudadanía) from Colombia
        /// Rule: 6 to 10 digits, only numbers
        /// </summary>
        /// <param name="document"> document to validate </param>
        /// <returns></returns>
        public static bool ValidateCC(string document)
        {
            document = document?.UnmaskDocument();

            // Verifica se a cédula tem entre 6 e 10 dígitos
            Regex regex = new Regex(@"^\d{6,10}$");
            return regex.IsMatch(document);
        }
    }
}
