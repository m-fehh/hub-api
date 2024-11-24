using Newtonsoft.Json;
using System.Dynamic;

namespace Hub.Infrastructure.Extensions
{
    public static class ExpandoObjectExtensions
    {
        public static T Map<T>(this ExpandoObject expando)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(expando));
        }

        public static ExpandoObject Clone(this ExpandoObject original)
        {
            var clone = new ExpandoObject();

            var _original = (IDictionary<string, object>)original;
            var _clone = (IDictionary<string, object>)clone;

            foreach (var kvp in _original)
                _clone.Add(kvp);

            return clone;
        }
    }
}
