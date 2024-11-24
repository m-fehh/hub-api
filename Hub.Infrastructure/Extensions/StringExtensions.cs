using EmailValidation;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Hub.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Efetua a redução do html removendo espaços e enters indesejados
        /// </summary>
        public static string MinifierHtml(this string html)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}|(?=[\r])\s{2,}");

            html = reg.Replace(html, string.Empty);

            html = html.Replace("'", @"\'");

            html = html.Replace("\n", string.Empty);

            return html;
        }

        /// <summary>
        /// Função que não permite que o texto seja maior que o tamanho especificado. Se for, corta e retorna com reticencias
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RestrictLength(this string text, int length)
        {
            if (text.Length <= length)
            {
                return text;
            }
            else
            {
                var reluctance = "...";

                if (length <= 3)
                {
                    return reluctance;
                }

                return text.Substring(0, length - 3) + reluctance;
            }
        }

        /// <summary>
        /// efetua o replace da string para remover os acentos
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveAccents(this string text)
        {

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Replace("Á", "A")
                    .Replace("É", "E")
                    .Replace("Í", "I")
                    .Replace("Ó", "O")
                    .Replace("Ú", "U")
                    .Replace("á", "a")
                    .Replace("é", "e")
                    .Replace("í", "i")
                    .Replace("ó", "o")
                    .Replace("ú", "u")

                    .Replace("À", "A")
                    .Replace("à", "a")
                    .Replace("È", "E")
                    .Replace("è", "e")
                    .Replace("Ì", "I")
                    .Replace("ì", "i")
                    .Replace("Ò", "O")
                    .Replace("ò", "o")
                    .Replace("Ù", "U")
                    .Replace("ù", "u")

                    .Replace("Â", "A")
                    .Replace("â", "a")
                    .Replace("Ê", "E")
                    .Replace("ê", "e")
                    .Replace("Î", "I")
                    .Replace("î", "i")
                    .Replace("Ô", "O")
                    .Replace("ô", "o")
                    .Replace("Û", "U")
                    .Replace("û", "u")

                    .Replace("Ä", "A")
                    .Replace("ä", "a")
                    .Replace("Ë", "E")
                    .Replace("ë", "e")
                    .Replace("Ï", "I")
                    .Replace("ï", "i")
                    .Replace("Ö", "O")
                    .Replace("ö", "o")
                    .Replace("Ü", "U")
                    .Replace("ü", "u")

                    .Replace("Ã", "A")
                    .Replace("Õ", "O")
                    .Replace("ã", "a")
                    .Replace("õ", "o")
                    .Replace("ñ", "n")

                    .Replace("Ç", "C")
                    .Replace("ç", "c")

                    .Replace("º", ".")
                    .Replace("°", ".")
                    .Replace("ª", ".")

                    .Replace("¿", "?")
                    .Replace("¡", "!")

                    .Replace("'", "")
                    .Replace("#", "");
            }
            else
            {
                return "";
            }

        }

        /// <summary>
        ///     Efetua o replace da string para remover os caracteres especiais e números
        /// </summary>
        /// <param cref="string" name="text">Texto que irá ser dado o replace</param>
        /// <returns>Retorna o texto sem caracteres especiais e sem números</returns>
        public static string RemoveSpecialCharactersAndNumbers(this string text)
        {
            return RemoveSpecialCharactersAlsoAccentsAlsoNumbers(text, false, true);
        }

        public static string RemoveSpecialCharactersAlsoAccentsAlsoNumbers(this string text, bool removeAccents = false, bool removeNumbers = false)
        {
            // Remove todos os números
            if (removeNumbers)
            {
                text = Regex.Replace(text, @"[0-9]+", "");
            }

            // Remove todos os caracteres especiais
            text = Regex.Replace(text, @"[^a-zA-ZÀ-ȕ\t\n\w\s]+", "");

            if (removeAccents)
            {
                text = RemoveAccents(text);
            }

            // Remove todos os espaçamentos extras (incluí tab, newline e dois ou mais whitespaces)
            return Regex.Replace(text, @"\s{2,}", " ").Trim();
        }

        public static bool IsInteger(this string text)
        {
            bool res = true;

            try
            {
                if (!string.IsNullOrEmpty(text) && ((text.Length != 1) || (text != "-")))
                {
                    Int64 i = Int64.Parse(text, CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                res = false;
            }
            return res;
        }

        public static bool IsDecimal(this string text)
        {
            bool res = true;

            try
            {
                if (!string.IsNullOrEmpty(text) && ((text.Length != 1) || (text != "-")))
                {
                    Decimal i = Decimal.Parse(text, CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                res = false;
            }
            return res;
        }

        public static string RemovePrefix(this string o, string prefix)
        {
            if (prefix == null) return o;
            return !o.StartsWith(prefix) ? o : o.Remove(0, prefix.Length);
        }

        public static string RemoveSuffix(this string o, string suffix)
        {
            if (suffix == null) return o;
            return !o.EndsWith(suffix) ? o : o.Remove(o.Length - suffix.Length, suffix.Length);
        }

        public static string ReturnNumeric(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(s, "");
        }

        public static string EncodeSHA1(this string s)
        {
            return Encoding.Unicode.GetString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(s)));
        }

        public static bool ValidaCnpj(this string cnpj)
        {
            cnpj = cnpj.OnlyNumbers();

            if (!cnpj.All(char.IsNumber))
            {
                return false;
            }

            if (cnpj.Length != 14)
                return false;

            switch (cnpj)
            {
                case "00000000000000":
                    return false;
                case "11111111111111":
                    return false;
                case "22222222222222":
                    return false;
                case "33333333333333":
                    return false;
                case "44444444444444":
                    return false;
                case "55555555555555":
                    return false;
                case "66666666666666":
                    return false;
                case "77777777777777":
                    return false;
                case "88888888888888":
                    return false;
                case "99999999999999":
                    return false;
            }

            if (!cnpj.All(char.IsNumber))
            {
                return false;
            }

            int[] multiplier1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum;
            int remainder;
            string digit;
            string tempCnpj;

            tempCnpj = cnpj.Substring(0, 12);
            sum = 0;
            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];
            remainder = (sum % 11);
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;
            digit = remainder.ToString();
            tempCnpj = tempCnpj + digit;
            sum = 0;
            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];
            remainder = (sum % 11);
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;
            digit = digit + remainder.ToString();
            return cnpj.EndsWith(digit);
        }

        public static bool ValidaCpf(this string cpf)
        {
            cpf = cpf.OnlyNumbers();

            if (cpf.Length != 11)
                return false;

            switch (cpf)
            {
                case "00000000000":
                    return false;
                case "11111111111":
                    return false;
                case "22222222222":
                    return false;
                case "33333333333":
                    return false;
                case "44444444444":
                    return false;
                case "55555555555":
                    return false;
                case "66666666666":
                    return false;
                case "77777777777":
                    return false;
                case "88888888888":
                    return false;
                case "99999999999":
                    return false;
            }

            int[] multiplier1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf;
            string digit;
            int sum;
            int remainder;

            if (!cpf.All(char.IsNumber))
            {
                return false;
            }

            if (cpf.Length != 11)
                return false;

            tempCpf = cpf.Substring(0, 9);
            sum = 0;

            for (int i = 0; i < 9; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

            remainder = sum % 11;

            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            digit = remainder.ToString();
            tempCpf = tempCpf + digit;
            sum = 0;

            for (int i = 0; i < 10; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            digit += remainder.ToString();

            return cpf.EndsWith(digit);
        }

        /// <summary>
        ///     Metodo de validação de RUT com dígito verificador dentro do string.
        /// </summary>
        /// <param name="rut" cref="string">RUT String</param>
        /// <returns cref="bool">Retorna se o RUT está correto</returns>
        public static bool ValidaRut(this string rut)
        {
            rut = rut.GenerateRUTEmissor();
            Regex expresion = new Regex("^([0-9]+-[0-9K])$");
            string dv = rut.Substring(rut.Length - 1, 1);
            if (!expresion.IsMatch(rut))
            {
                return false;
            }
            char[] charCorte = { '-' };
            string[] rutTemp = rut.Split(charCorte);
            if (dv != DigitoRut(int.Parse(rutTemp[0])))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Gera o valor RUT no formato correto para ser enviado para o parceiro Facele Chile.
        /// </summary>
        /// <param name="rutEmissor" cref="string">RUT do Emissor</param>
        /// <returns cref="string">RUT no formato correto</returns>
        public static string GenerateRUTEmissor(this string rutEmissor)
        {
            return rutEmissor.Replace(".", "").Replace("-", "").Trim().Insert(rutEmissor.Length - 1, "-").ToUpper();
        }

        /// <summary>
        ///     Método que calcula o dígito verificador a partir do RUT enviado.
        /// </summary>
        /// <param name="rut" cref="int">RUT Inteiro</param>
        /// <returns cref="string">Retorna dígito verificador</returns>
        public static string DigitoRut(int rut)
        {
            int suma = 0;
            int multiplicador = 1;
            while (rut != 0)
            {
                multiplicador++;
                if (multiplicador == 8) { multiplicador = 2; }
                suma += (rut % 10) * multiplicador;
                rut = rut / 10;
            }
            suma = 11 - (suma % 11);
            if (suma == 11)
            {
                return "0";
            }
            else if (suma == 10)
            {
                return "K";
            }
            else
            {
                return suma.ToString();
            }
        }

        public static string StringOnly(this string value)
        {
            return Regex.Replace(value, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
        }

        public static string ReplaceSpecialCharacters(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            return Regex.Replace(s, @"[^0-9a-zA-Z\s]+", "");
        }

        /// <summary>
        /// Valida um email utilizando a biblioteca EmailValidation
        /// </summary>
        /// <param name="email">Email a ser validado</param>
        /// <returns>Verdadeiro se válido</returns>
        public static bool ValidateEmail(this string email)
        {
            return EmailValidator.Validate(email);
        }


        public static bool ValidateIp(this string email)
        {
            Regex rg = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

            if (rg.IsMatch(email))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ValidateDomain(this string domain)
        {
            Regex rg = new Regex(@"^@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]+[A-Za-z0-9])$");

            if (rg.IsMatch(domain))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsValidGuid(this string guid)
        {
            var newGuid = Guid.Empty;
            Guid.TryParse(guid, out newGuid);

            if (newGuid == Guid.Empty)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string OnlyNumbers(this string phone)
        {
            var numbers = new Regex(@"[^\d]");
            return numbers.Replace(phone, "");
        }

        public static long? ToNullableLong(this string number)
        {
            if (long.TryParse(number, out long output))
                return output;

            return null;
        }

        public static long ToLong(this string number)
        {
            if (long.TryParse(number, out long output))
                return output;

            return 0;
        }

        public static string FirstName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            var firstName = name.Split(' ')[0];

            if (firstName.Length == 1)
            {
                return firstName.ToUpper();
            }
            else
            {
                return string.Concat(char.ToUpper(firstName[0]), firstName.Substring(1).ToLower());
            }
        }

        public static string GetLast(this string source, int tail_length)
        {
            if (source == null) return null;

            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public static string PascalToKebabCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }

        /// <summary>
        /// Tenta converter uma string em int com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static int TryParseInt(this string s, int fallback)
        {
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out int result))
                return result;

            return fallback;
        }

        /// <summary>
        /// Tenta converter uma string em long com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static long TryParseLong(this string? s, long fallback)
        {
            if (!string.IsNullOrEmpty(s) && long.TryParse(s, out long result))
                return result;

            return fallback;
        }

        /// <summary>
        /// Tenta converter uma string em decimal com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static decimal TryParseDecimal(this string? s, decimal fallback)
        {
            if (!string.IsNullOrEmpty(s) && decimal.TryParse(s, out decimal result))
                return result;

            return fallback;
        }

        /// <summary>
        /// Tenta converter uma string em bool com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static bool TryParseBoolean(this string? s, bool fallback)
        {
            if (!string.IsNullOrEmpty(s) && bool.TryParse(s, out bool result))
                return result;

            return fallback;
        }

        /// <summary>
        /// Tenta converter uma string em data com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <param name="format">Formato exato</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static DateTime TryParseDateTime(this string? s, DateTime fallback, string? format = null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(format))
                {
                    if (DateTime.TryParse(s, out DateTime result))
                        return result;
                }
                else
                {
                    if (DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out DateTime result))
                        return result;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Tenta converter uma string em um Enum com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static TEnum TryParseEnum<TEnum>(this string? s, TEnum fallback) where TEnum : Enum
        {
            if (!string.IsNullOrEmpty(s) && Enum.TryParse(typeof(TEnum), s, true, out object? result))
                return (TEnum)result;

            return fallback;
        }


        /// <summary>
        /// Tenta converter uma string em timespan com opção de valor padrão
        /// </summary>
        /// <param name="s">Texto a ser convertido</param>
        /// <param name="fallback">Valor padrão</param>
        /// <param name="format">Formato exato</param>
        /// <returns>Valor da conversão ou valor padrão</returns>
        public static TimeSpan TryParseTimeSpan(this string? s, TimeSpan fallback, string? format = null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(format))
                {
                    if (TimeSpan.TryParse(s, out TimeSpan result))
                        return result;
                }
                else
                {
                    if (TimeSpan.TryParseExact(s, format, null, out TimeSpan result))
                        return result;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Rotina responsável por validar três tipos de documentos:
        /// - CPF quando o tamanho é de 11 caracteres.
        /// - CNPJ quando o tamanho é de 14 caracteres.
        /// - RNE e NIT será assumido quando o tamanho estiver entre 7 e 10 caracteres.
        /// </summary>
        /// <param name="value">Documento do usuário para validação</param>
        /// <returns>Válido / Inválido</returns>
        public static bool ValidateDocument(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            var cleanVal = value.ToString().Replace(".", "").Replace("-", "").Replace("/", "");

            // CPF
            if (cleanVal.Length == 11)
                return ValidaCpf(cleanVal);
            // CNPJ
            else if (cleanVal.Length == 14)
                return ValidaCnpj(cleanVal);
            // RNE e NIT
            else if (cleanVal.Length >= 7 && cleanVal.Length <= 10)
                return true;
            else
                return false;
        }

        public static bool HasSpecialCharacters(this string s)
        {
            var regex = new Regex(@"[^0-9a-zA-Z\s]+");
            return regex.IsMatch(s);
        }

        public static bool HasSpecialCharactersOrNumbers(this string s)
        {
            var regex = new Regex(@"[^a-zA-Z\s]+");
            return regex.IsMatch(s);
        }

        /// <summary>
        /// Converte uma string no formato de data brasileira (dd/MM/yyyy) para DateTime
        /// </summary>
        /// <param name="date">Data no formato brasileiro (dd/MM/yyyy)</param>
        /// <returns>Data convertida em DateTime</returns>
        public static DateTime FromBrazilianDateFormat(this string date)
        {
            return DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Validação de data brasileira (dd/MM/yyyy) para DateTime
        /// </summary>
        /// <param name="date">string data</param>
        /// <returns>verdadeiro caso data válida</returns>
        public static bool IsValidBrazillianDate(this string date)
        {
            var pattern = @"^(0?[1-9]?|[1-2][0-9]|3[0-1])\/(0?[1-9]?|1[0-2])\/(19[0-9]{2}|20[0-9]{2})$";

            var match = Regex.Match(date, pattern);

            if (match.Success && match.Groups.Count == 4)
            {
                int.TryParse(match.Groups[1].Value, out int d);
                int.TryParse(match.Groups[2].Value, out int m);
                int.TryParse(match.Groups[3].Value, out int y);

                // fevereiro
                if (m == 2)
                    return d < 30;

                // meses com 30 dias
                if (m == 4 || m == 6 || m == 9 || m == 11)
                    return d < 31;
            }

            return match.Success;
        }

        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
            }
        }

        /// <summary>
        /// Verifica se o campo possui pelo menos duas partículas
        /// </summary>
        /// <returns></returns>
        public static bool ValidateFullName(this string fullName)
        {
            string pattern = @"(\w.+\s).+";

            Regex regex = new Regex(pattern);

            return regex.IsMatch(fullName);
        }

        public static string PhoneFormat(this string phone, bool? mask = false)
        {
            // em alguns casos o número de telefone retorna do banco já formato como hidden
            if (phone.Contains("*"))
            {
                return "** (**) *****-****";
            }

            var count = phone.Trim().Length;

            var phoneFormat = "## (##) #####-####";

            switch (count)
            {
                // ####-#### residencial
                case 8:
                    phoneFormat = "####-####";
                    break;
                // #####-#### celular
                case 9:
                    phoneFormat = "#####-####";
                    break;
                // (##) ####-#### residencial com DDD
                case 10:
                    phoneFormat = "(##) ####-####";
                    break;
                // (##) #####-#### celular com DDD
                case 11:
                    phoneFormat = "(##) #####-####";
                    break;
                // ## (##) ####-#### residencial com DDI e DDD
                case 12:
                    phoneFormat = "## (##) ####-####;";
                    break;
                // ## (##) #####-#### celular com DDI e DDD
                case 13:
                    phoneFormat = "## (##) #####-####";
                    break;
                default:
                    phoneFormat = "";
                    break;
            }

            long number;

            if (!long.TryParse(phone, out number))
            {
                return phone;
            }

            if (mask == true)
            {
                var maskedPhone = Convert.ToInt64(phone).ToString(phoneFormat);

                var finalFourDigits = maskedPhone.Substring(maskedPhone.Length - 4);

                maskedPhone = Regex.Replace(maskedPhone.Substring(0, maskedPhone.Length - 4), @"[\d+]", "*");

                maskedPhone = maskedPhone + finalFourDigits;

                return maskedPhone;
            }

            return Convert.ToInt64(phone).ToString(phoneFormat);
        }

        public static bool IsHourMinuteTimeFormat(this string time)
        {

            var pattern = @"^(0*[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$";


            var match = Regex.Match(time, pattern);


            return match.Success;

        }
    }

    public static class StringCompressor
    {
        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(this string text)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);

            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);

                var outputBytes = outputStream.ToArray();

                return Convert.ToBase64String(outputBytes);
            }
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(this string compressedText)
        {
            try
            {
                byte[] inputBytes = Convert.FromBase64String(compressedText);

                using (var inputStream = new MemoryStream(inputBytes))
                using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var streamReader = new StreamReader(gZipStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (InvalidDataException)
            {
                //acontece que mudamos a forma de compactar, e para os arquivos legado, esse bloco de código faz o modelo antigo de descompactação

                byte[] gZipBuffer = Convert.FromBase64String(compressedText);
                using (var memoryStream = new MemoryStream())
                {
                    int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                    memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                    var buffer = new byte[dataLength];

                    memoryStream.Position = 0;
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        gZipStream.Read(buffer, 0, buffer.Length);
                    }

                    return Encoding.UTF8.GetString(buffer);
                }
            }


        }
    }
}
