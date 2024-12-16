using Newtonsoft.Json.Linq;

namespace Hub.Infrastructure.Internationalization.ProviderPostalCode
{
    public interface IProviderPostalCode
    {
        JObject Search(string postalCode);
    }

    public static class GetProviderPostalCode
    {
        public static JObject GetPostalCode(EProviderPostalCode provider, string code)
        {
            IProviderPostalCode postalCode = Create(provider.ToString());
            var obj = postalCode.Search(code);
            return obj;
        }
        private static IProviderPostalCode Create(string provider)
        {
            try
            {
                provider = $"FMK.Core.Internationalization.ProviderPostalCode.{provider.ToUpper()}";
                Type type = Type.GetType(provider);
                return (IProviderPostalCode)Activator.CreateInstance(type);
            }
            catch { return null; }
        }

    }
}
