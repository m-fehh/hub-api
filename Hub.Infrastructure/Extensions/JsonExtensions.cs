using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Hub.Infrastructure.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Transforma um objeto qualquer em uma stirng no formato JSON
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new IsoDateTimeConverter() { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeLocal });
        }

        /// <summary>
        /// Transforma uma string no formato JSON em algum tipo de objeto
        /// </summary>
        /// <typeparam name="T">o tipo de objeto correspondente em que a strng passada será transformada</typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeJSON<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new IsoDateTimeConverter());
        }
    }

    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //assume we can convert to anything for now
            return true;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //explicitly specify the concrete type we want to create
            return serializer.Deserialize<TConcrete>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //use the default serialization - it works fine
            serializer.Serialize(writer, value);
        }
    }

    public class ConcreteListTypeConverter<TInterface, TImplementation> : JsonConverter where TImplementation : TInterface
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var res = serializer.Deserialize<List<TImplementation>>(reader);
            return res.ConvertAll(x => (TInterface)x);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
