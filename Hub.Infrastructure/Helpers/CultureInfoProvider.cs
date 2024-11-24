using System.Globalization;
using System.Text.RegularExpressions;

namespace Hub.Infrastructure.Helpers
{
    public class CultureInfoProvider
    {
        private const string LanguagePattern = @"^[a-z]{2}-[A-Z]{2}$";

        /// <summary>
        /// Sets the culture info based on the provided language code.
        /// </summary>
        /// <param name="languageCode">The language code (e.g., "en-US" or "pt-BR").</param>
        /// <returns>The validated language code.</returns>
        public static string SetCultureInfo(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode) || !Regex.IsMatch(languageCode, LanguagePattern))
            {
                languageCode = CultureInfo.CurrentCulture.Name;
            }

            return languageCode;
        }

        /// <summary>
        /// Sets the default culture info for the system based on configuration.
        /// A default culture is set and applied to the current thread.
        /// </summary>
        public static void SetDefaultCultureInfo()
        {
            string defaultCulture = Engine.AppSettings["defaultCulture"] ?? "pt-BR";
            if (defaultCulture.Contains("en"))
            {
                defaultCulture = "en-US";
            }
            else
            {
                defaultCulture = "pt-BR";
            }

            CultureInfo cultureInfo = new CultureInfo(defaultCulture);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }
    }
}
