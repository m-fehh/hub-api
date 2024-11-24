using Newtonsoft.Json.Linq;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.Json;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Hub.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings() { Indent = false, NewLineChars = "" }))
                {
                    xmlSerializer.Serialize(xmlWriter, toSerialize);

                    return HttpUtility.HtmlEncode(textWriter.ToString());
                }
            }
        }

        public static byte[] ImageToByteArray(this Image imageIn)
        {
            if (imageIn == null) return null;

            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public static Image ByteArrayToImage(this byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static byte[] ConvertToByteArray(this Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static byte[] ObjectToByteArray(this object obj)
        {
            if (obj == null)
                return null;

            // Serializa o objeto em JSON e converte para um array de bytes
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public static T ByteArrayToObject<T>(this byte[] arrBytes)
        {
            if (arrBytes == null || arrBytes.Length == 0)
                throw new ArgumentNullException(nameof(arrBytes));

            // Desserializa o array de bytes em um objeto do tipo T
            return JsonSerializer.Deserialize<T>(arrBytes);
        }

        /// <summary>
        /// Copia apenas as propriedades que possuirem o mesmo nome do objeto json para o objeto c# destino
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="ignoreProperties"></param>
        public static void Merge<T>(this T target, JObject source, List<string> ignoreProperties = null)
        {
            var targetProperties = typeof(T).GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            var sourceProperties = source.Properties();

            var sourceConverted = source.ToObject<T>();

            foreach (var prop in targetProperties)
            {
                if ((ignoreProperties != null && ignoreProperties.Any(p => p == prop.Name)) || !sourceProperties.Any(p => p.Name == prop.Name)) continue;

                var value = prop.GetValue(sourceConverted, null);

                prop.SetValue(target, value, null);
            }
        }

        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

        public static DataTable ToDataTable(this IEnumerable<dynamic> items)
        {
            var data = items.ToArray();
            if (data.Count() == 0) return null;

            var dt = new DataTable();

            foreach (var key in ((IDictionary<string, object>)data[0]).Keys)
            {
                var value = items.Where(a => ((IDictionary<string, object>)a)[key] != null).Select(a => ((IDictionary<string, object>)a)[key]).FirstOrDefault();

                if (value != null)
                {
                    dt.Columns.Add(key, value.GetType());
                }
                else
                {
                    dt.Columns.Add(key);
                }
            }
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }

            return dt;
        }

        public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int nSize = 30)
        {
            for (int i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }
    }
}
