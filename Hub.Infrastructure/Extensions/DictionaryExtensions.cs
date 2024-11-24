using System.Collections;
using System.Collections.Specialized;

namespace Hub.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        public static NameValueCollection ToNameValueCollection(this IDictionary dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (DictionaryEntry kvp in dict)
            {
                string value = null;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}
