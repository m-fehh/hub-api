using Hub.Infrastructure.Extensions;

namespace Hub.Application.Validators
{
    public interface IDocumentValidator
    {
        bool IsValid(string document);
    }

    public class DocumentValidator : IDocumentValidator
    {
        /// <summary>
        /// Método responsável por verificar a validade de um CPF/CNPJ
        /// </summary>
        /// <param name="document"> documento </param>
        /// <returns> true: válido; false: inválido </returns>
        public bool IsValid(string document)
        {
            var isValid = false;

            switch (document.Length)
            {
                case 11:
                    isValid = document.ValidaCpf();
                    break;

                case 14:
                    isValid = document.ValidaCnpj();
                    break;
            }

            return isValid;
        }
    }
}
