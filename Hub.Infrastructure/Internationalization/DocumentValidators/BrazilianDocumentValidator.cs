using Hub.Infrastructure.Extensions;

namespace Hub.Infrastructure.Internationalization.DocumentValidators
{
    /// <summary>
    /// Validator para documentos do Brasil
    /// </summary>
    public static class BrazilianDocumentValidator
    {
        /// <summary>
        /// Valida um documento CPF
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        public static bool ValidateCPF(string document)
        {
            document = document?.UnmaskDocument();

            // Verifica se o CPF tem 11 dígitos
            if (document.Length != 11) return false;

            // Verifica se todos os dígitos são iguais (CPF inválido)
            if (document.All(c => c == document[0])) return false;

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += (document[i] - '0') * (10 - i);
            }

            int primeiroDigito = (soma % 11) < 2 ? 0 : 11 - (soma % 11);

            // Verifica o primeiro dígito verificador
            if (primeiroDigito != (document[9] - '0')) return false;

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += (document[i] - '0') * (11 - i);
            }

            int segundoDigito = (soma % 11) < 2 ? 0 : 11 - (soma % 11);

            // Verifica o segundo dígito verificador
            if (segundoDigito != (document[10] - '0')) return false;

            return true;
        }

        /// <summary>
        /// Valida um documento CNPJ
        /// </summary>
        /// <param name="document"> documento a ser validado </param>
        /// <returns></returns>
        public static bool ValidateCNPJ(string document)
        {
            document = document?.UnmaskDocument();

            // Verifica se o documento tem 14 dígitos
            if (document.Length != 14) return false;

            // Verifica se todos os dígitos são iguais (documento inválido)
            if (document.All(c => c == document[0])) return false;

            // Peso para o cálculo dos dígitos verificadores
            int[] pesosPrimeiroDigito = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesosSegundoDigito = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Cálculo do primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 12; i++)
            {
                soma += (document[i] - '0') * pesosPrimeiroDigito[i];
            }

            int primeiroDigito = (soma % 11) < 2 ? 0 : 11 - (soma % 11);

            // Verifica o primeiro dígito
            if (primeiroDigito != (document[12] - '0')) return false;

            // Cálculo do segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 13; i++)
            {
                soma += (document[i] - '0') * pesosSegundoDigito[i];
            }

            int segundoDigito = (soma % 11) < 2 ? 0 : 11 - (soma % 11);

            // Verifica o segundo dígito
            if (segundoDigito != (document[13] - '0')) return false;

            return true;
        }
    }
}
