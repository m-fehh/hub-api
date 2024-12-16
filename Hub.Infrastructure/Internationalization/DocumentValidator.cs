using Hub.Infrastructure.Internationalization.DocumentValidators;
using Hub.Shared.Enums;
using System.Runtime.CompilerServices;

namespace Hub.Infrastructure.Internationalization
{
    /// <summary>
    /// Validates documents.
    /// </summary>
    public static class DocumentValidator
    {
        /// <summary>
        /// Validates the specified document based on the provided validation type.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="validation">The type of validation to perform.</param>
        /// <returns>True if the document is valid; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool ValidateDocument(this string document, ESpecialDocumentValidation validation)
        {
            return Validate(document, validation);
        }
        /// <summary>
        /// Validates the specified document based on the Phone Number country.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="systemNumber">The Phone Number</param>
        /// <returns>True if the document is valid; otherwise, false.</returns>
        public static bool ValidateDocumentUsingPhoneNumber(this string document, string systemNumber)
        {
            string country = PhoneNumberValidator.GetCountryCodeForNumber(systemNumber);
            ESpecialDocumentValidation documentType = GetDocumentTypeForCountry(country);
            return Validate(document, documentType);
        }
        /// <summary>
        /// Validates the specified document based on the provided validation type.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="validation">The type of validation to perform.</param>
        /// <returns>True if the document is valid; otherwise, false.</returns>
        public static bool Validate(string document, ESpecialDocumentValidation validation)
        {
            switch (validation)
            {
                case ESpecialDocumentValidation.None:
                    return !string.IsNullOrEmpty(document);
                case ESpecialDocumentValidation.CPF:
                    return BrazilianDocumentValidator.ValidateCPF(document);
                case ESpecialDocumentValidation.CNPJ:
                    return BrazilianDocumentValidator.ValidateCNPJ(document);
                case ESpecialDocumentValidation.ChileanRUT:
                    return ChileanDocumentValidator.ValidateRUT(document);
                case ESpecialDocumentValidation.ColombianCC:
                    return ColombianDocumentValidator.ValidateCC(document);
                case ESpecialDocumentValidation.ArgentineDNI:
                    return ArgentineDocumentValidator.ValidateDNI(document);
                case ESpecialDocumentValidation.ArgentineCUIT:
                    return ArgentineDocumentValidator.ValidateCUIT(document);
                case ESpecialDocumentValidation.ParaguayanCI:
                    return ParaguayanDocumentValidator.ValidateCI(document);
                case ESpecialDocumentValidation.EUASSN:
                    return EUADocumentValidator.IsValidSSN(document);
                default:
                    return false;
            }
        }
        public static ESpecialDocumentValidation GetDocumentTypeForCountry(string country)
        {
            var countryDocuments = new Dictionary<string, ESpecialDocumentValidation>
            {
                { "US", ESpecialDocumentValidation.EUASSN }, // Estados Unidos (Social Security Number)
                { "BR", ESpecialDocumentValidation.CPF }, // Brasil (Cadastro de Pessoas Físicas)
                { "AR", ESpecialDocumentValidation.ArgentineDNI }, // Argentina (Documento Nacional de Identidad)
                { "CL", ESpecialDocumentValidation.ChileanRUT }, // Chile (Rol Único Nacional)
                { "PE", ESpecialDocumentValidation.ArgentineDNI }, // Peru  (Documento Nacional de Identidad)
                { "CO", ESpecialDocumentValidation.ColombianCC }, // Colômbia
                { "VE", ESpecialDocumentValidation.ColombianCC }, // Venezuela
                { "UY", ESpecialDocumentValidation.ColombianCC }, // Uruguai
                { "PY", ESpecialDocumentValidation.ColombianCC }, // Paraguai
                { "BO", ESpecialDocumentValidation.ColombianCC }, // Bolívia
                { "EC", ESpecialDocumentValidation.ColombianCC}, // Equador
            };

            if (countryDocuments.TryGetValue(country, out ESpecialDocumentValidation documentName))
                return documentName;
            else
                return ESpecialDocumentValidation.None;
        }
    }
}
