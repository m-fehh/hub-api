﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Internationalization
{
    public static class PostalCodeValidator
    {
        /// <summary>
        /// Validates the specified country based on the code regex.
        /// </summary>
        /// <param name="country">The country to validate.</param>
        /// <param name="zipCode">The zipcode for validation.</param>
        /// <returns>True if the zipCode of Country is valid according to the regex code; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool ValidatePostalCode(this string country, string zipCode)
        {
            return Validate(country, zipCode);
        }

        private static bool Validate(string country, string zipCode)
        {
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(zipCode)) return false;

            #region Dicionário Codigo regex
            Dictionary<string, string> dicRegex = new Dictionary<string, string>()
            {
                {"BR", "\\d{5}[\\-]?\\d{3}"},
                {"AR", "([A-HJ-NP-Z])?\\d{4}([A-Z]{3})?"},
                {"CL", "\\d{7}"},
                {"PY", "\\d{4}"},
                {"CO", "\\d{6}"},
                {"CR", "\\d{4,5}|\\d{3}-\\d{4}"},
                {"JE", "JE\\d[\\dA-Z]?[ ]?\\d[ABD-HJLN-UW-Z]{2}"},
                {"GG", "GY\\d[\\dA-Z]?[ ]?\\d[ABD-HJLN-UW-Z]{2}"},
                {"IM", "IM\\d[\\dA-Z]?[ ]?\\d[ABD-HJLN-UW-Z]{2}"},
                {"US", "\\d{5}([ \\-]\\d{4})?"},
                {"CA", "[ABCEGHJKLMNPRSTVXY]\\d[ABCEGHJ-NPRSTV-Z][ ]?\\d[ABCEGHJ-NPRSTV-Z]\\d"},
                {"DE", "\\d{5}"},
                {"JP", "\\d{3}-\\d{4}"},
                {"FR", "\\d{2}[ ]?\\d{3}"},
                {"AU", "\\d{4}"},
                {"IT", "\\d{5}"},
                {"CH", "\\d{4}"},
                {"AT", "\\d{4}"},
                {"ES", "\\d{5}"},
                {"NL", "\\d{4}[ ]?[A-Z]{2}"},
                {"BE", "\\d{4}"},
                {"DK", "\\d{4}"},
                {"SE", "\\d{3}[ ]?\\d{2}"},
                {"NO", "\\d{4}" },
                {"PT", "\\d{4}([\\-]\\d{3})?"},
                {"FI", "\\d{5}"},
                {"AX", "22\\d{3}"},
                {"KR", "\\d{3}[\\-]\\d{3}"},
                {"CN", "\\d{6}"},
                {"TW", "\\d{3}(\\d{2})?"},
                {"SG", "\\d{6}"},
                {"DZ", "\\d{5}"},
                {"AD", "AD\\d{3}"},
                {"AM", "(37)?\\d{4}"},
                {"AZ", "\\d{4}"},
                {"BH", "((1[0-2]|[2-9])\\d{2})?"},
                {"BD", "\\d{4}"},
                {"BB", "(BB\\d{5})?"},
                {"BY", "\\d{6}"},
                {"BM", "[A-Z]{2}[ ]?[A-Z0-9]{2}"},
                {"BA", "\\d{5}"},
                {"IO", "BBND 1ZZ"},
                {"BN", "[A-Z]{2}[ ]?\\d{4}"},
                {"BG", "\\d{4}"},
                {"KH", "\\d{5}"},
                {"CV", "\\d{4}"},
                {"HR", "\\d{5}"},
                {"CY", "\\d{4}"},
                {"CZ", "\\d{3}[ ]?\\d{2}"},
                {"DO", "\\d{5}"},
                {"EC", "([A-Z]\\d{4}[A-Z]|(?:[A-Z]{2})?\\d{6})?"},
                {"EG", "\\d{5}"},
                {"EE", "\\d{5}"},
                {"FO", "\\d{3}"},
                {"GE", "\\d{4}"},
                {"GR", "\\d{3}[ ]?\\d{2}"},
                {"GL", "39\\d{2}"},
                {"GT", "\\d{5}"},
                {"HT", "\\d{4}"},
                {"HN", "(?:\\d{5})?"},
                {"HU", "\\d{4}"},
                {"IS", "\\d{3}"},
                {"IN", "\\d{6}"},
                {"ID", "\\d{5}"},
                {"IL", "\\d{5}"},
                {"JO", "\\d{5}"},
                {"KZ", "\\d{6}"},
                {"KE", "\\d{5}"},
                {"KW", "\\d{5}"},
                {"LA", "\\d{5}"},
                {"LV", "\\d{4}"},
                {"LB", "(\\d{4}([ ]?\\d{4})?)?"},
                {"LI", "(948[5-9])|(949[0-7])"},
                {"LT", "\\d{5}"},
                {"LU", "\\d{4}"},
                {"MK", "\\d{4}"},
                {"MY", "\\d{5}"},
                {"MV", "\\d{5}"},
                {"MT", "[A-Z]{3}[ ]?\\d{2,4}"},
                {"MU", "(\\d{3}[A-Z]{2}\\d{3})?"},
                {"MX", "\\d{5}"},
                {"MD", "\\d{4}"},
                {"MC", "980\\d{2}"},
                {"MA", "\\d{5}"},
                {"NP", "\\d{5}"},
                {"NZ", "\\d{4}"},
                {"NI", "((\\d{4}-)?\\d{3}-\\d{3}(-\\d{1})?)?"},
                {"NG", "(\\d{6})?"},
                {"OM", "(PC )?\\d{3}"},
                {"PK", "\\d{5}"},
                {"PH", "\\d{4}"},
                {"PL", "\\d{2}-\\d{3}"},
                {"PR", "00[679]\\d{2}([ \\-]\\d{4})?"},
                {"RO", "\\d{6}"},
                {"RU", "\\d{6}"},
                {"SM", "4789\\d"},
                {"SA", "\\d{5}"},
                {"SN", "\\d{5}"},
                {"SK", "\\d{3}[ ]?\\d{2}"},
                {"SI", "\\d{4}"},
                {"ZA", "\\d{4}"},
                {"LK", "\\d{5}"},
                {"TJ", "\\d{6}"},
                {"TH", "\\d{5}"},
                {"TN", "\\d{4}"},
                {"TR", "\\d{5}"},
                {"TM", "\\d{6}"},
                {"UA", "\\d{5}"},
                {"UY", "\\d{5}"},
                {"UZ", "\\d{6}"},
                {"VA", "00120"},
                {"VE", "\\d{4}"},
                {"ZM", "\\d{5}"},
                {"AS", "96799"},
                {"CC", "6799"},
                {"CK", "\\d{4}"},
                {"RS", "\\d{6}"},
                {"ME", "8\\d{4}"},
                {"CS", "\\d{5}"},
                {"YU", "\\d{5}"},
                {"CX", "6798"},
                {"ET", "\\d{4}"},
                {"FK", "FIQQ 1ZZ"},
                {"NF", "2899"},
                {"FM", "(9694[1-4])([ \\-]\\d{4})?"},
                {"GF", "9[78]3\\d{2}"},
                {"GN", "\\d{3}"},
                {"GP", "9[78][01]\\d{2}"},
                {"GS", "SIQQ 1ZZ"},
                {"GU", "969[123]\\d([ \\-]\\d{4})?"},
                {"GW", "\\d{4}"},
                {"HM", "\\d{4}"},
                {"IQ", "\\d{5}"},
                {"KG", "\\d{6}"},
                {"LR", "\\d{4}"},
                {"LS", "\\d{3}"},
                {"MG", "\\d{3}"},
                {"MH", "969[67]\\d([ \\-]\\d{4})?"},
                {"MN", "\\d{6}"},
                {"MP", "9695[012]([ \\-]\\d{4})?"},
                {"MQ", "9[78]2\\d{2}"},
                {"NC", "988\\d{2}"},
                {"NE", "\\d{4}"},
                {"VI", "008(([0-4]\\d)|(5[01]))([ \\-]\\d{4})?"},
                {"PF", "987\\d{2}"},
                {"PG", "\\d{3}"},
                {"PM", "9[78]5\\d{2}"},
                {"PN", "PCRN 1ZZ"},
                {"PW", "96940"},
                {"RE", "9[78]4\\d{2}"},
                {"SH", "(ASCN|STHL) 1ZZ"},
                {"SJ", "\\d{4}"},
                {"SO", "\\d{5}"},
                {"SZ", "[HLMS]\\d{3}"},
                {"TC", "TKCA 1ZZ"},
                {"WF", "986\\d{2}"},
                {"XK", "\\d{5}"},
                {"YT", "976\\d{2}"}
            };
            #endregion

            string regexCode;
            dicRegex.TryGetValue(country, out regexCode);
            return Regex.IsMatch(zipCode, regexCode);
        }
    }
}
