using System.Data;

namespace Hub.Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> list) where T : ICloneable
        {
            if (list == null) return null;

            return list.Select(t => (T)t.Clone()).ToList();
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
    }
}
