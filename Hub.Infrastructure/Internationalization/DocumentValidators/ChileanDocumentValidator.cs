using Hub.Infrastructure.Extensions;

namespace Hub.Infrastructure.Internationalization.DocumentValidators
{
    /// <summary>
    /// Validator para documentos do Chile
    /// </summary>
    public static class ChileanDocumentValidator
    {
        /// <summary>
        /// Validates a RUTChile document.
        /// </summary>
        /// <param name="document">The RUTChile document to validate.</param>
        /// <returns>True if the RUTChile document is valid; otherwise, false.</returns>
        public static bool ValidateRUT(string document)
        {
            document = document?.UnmaskDocument();

            // Verifica se o document tem pelo menos 2 caracteres
            if (document.Length < 2) return false;

            // Separa o corpo do documento e o dígito verificador
            string corpo = document.Substring(0, document.Length - 1);
            char digitoVerificador = document[document.Length - 1];

            // Verifica se o corpo é numérico
            if (!corpo.All(char.IsDigit)) return false;

            // Calcula o dígito verificador esperado
            int soma = 0;
            int multiplicador = 2;

            // Itera pelos dígitos do corpo, de trás para frente
            for (int i = corpo.Length - 1; i >= 0; i--)
            {
                soma += (corpo[i] - '0') * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }

            int resto = soma % 11;
            char digitoEsperado = resto == 0 ? '0' : resto == 1 ? 'K' : (char)(11 - resto + '0');

            // Compara o dígito esperado com o fornecido
            return digitoVerificador == digitoEsperado;
        }
    }
}
