using System.Globalization;

namespace Hub.Infrastructure
{
    public interface ILocalizationProvider
    {
        string Get(string key);
        string Get(string key, CultureInfo culture);
        string GetByValue(string value);
    }
}
