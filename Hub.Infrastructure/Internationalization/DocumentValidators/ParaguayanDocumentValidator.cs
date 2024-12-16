using Hub.Infrastructure.Extensions;
using System.Text.RegularExpressions;

namespace Hub.Infrastructure.Internationalization.DocumentValidators
{
    /// <summary>
    /// Validator para documentos do Paraguai
    /// </summary>
    public static class ParaguayanDocumentValidator
    {
        /// <summary>
        /// Valida um documento CI do Paraguai
        /// Regra: 6 a 9 digitos, apenas números
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        public static bool ValidateCI(string document)
        {
            document = document.UnmaskDocument();

            // Verificar se o comprimento é 6 a 9
            if (document.Length < 6 || document.Length > 9)
            {
                return false;
            }

            // Verificar se todos os caracteres são dígitos
            if (!Regex.IsMatch(document, @"^\d+$"))
            {
                return false;
            }

            // Se tem 9 dígitos, validar dígito verificador (último dígito)
            if (document.Length == 9)
            {
                return ValidateCheckDigit(document);
            }

            return true;
        }

        /// <summary>
        /// Valida o digito verificador de um documento CI do Paraguai
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        private static bool ValidateCheckDigit(string document)
        {
            // Separar o número base do dígito verificador
            string numeroBase = document.Substring(0, 8);
            char digitoVerificador = document[8];

            // Calcular dígito verificador
            int soma = 0;
            for (int i = 0; i < numeroBase.Length; i++)
            {
                soma += (numeroBase[i] - '0') * (9 - i);
            }
            int digitoCalculado = (11 - (soma % 11)) % 10;

            // Comparar com o dígito verificador fornecido
            return digitoCalculado == (digitoVerificador - '0');
        }
    }
}
