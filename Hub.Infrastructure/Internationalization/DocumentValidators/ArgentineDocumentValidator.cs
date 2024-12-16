using Hub.Infrastructure.Extensions;
using System.Text.RegularExpressions;

namespace Hub.Infrastructure.Internationalization.DocumentValidators
{
    /// <summary>
    /// Validator para documentos da Argentina
    /// </summary>
    public static class ArgentineDocumentValidator
    {
        /// <summary>
        /// Valida documento DNI da Argentina
        /// Regra: 7 a 8 dígitos, apenas números
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        public static bool ValidateDNI(string document)
        {
            document = document?.UnmaskDocument();

            string pattern = @"^\d{7,8}$";
            return Regex.IsMatch(document, pattern);
        }

        /// <summary>
        /// Valida documento CUIT da Argentina
        /// Regra: 11 dígitos, apenas números, utiliza algoritmo de validação
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        public static bool ValidateCUIT(string document)
        {
            document = document?.UnmaskDocument();

            if (string.IsNullOrWhiteSpace(document) || document.Length != 11 || !long.TryParse(document, out _))
            {
                return false;
            }

            int[] pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(document[i].ToString()) * pesos[i];
            }

            int resto = soma % 11;
            int digitoVerificador = resto == 0 ? 0 : 11 - resto;

            return digitoVerificador == int.Parse(document[10].ToString());
        }
    }
}
