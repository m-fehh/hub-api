using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// Cria uma lista de DateTimeRange a partir de uma lista de datas. Conecta as datas subsequentes em um mesmo range.
        /// Por exemplo, para as datas 10/01, 11/01, 12/01, 13/01, 14/01, 17/01, 18/01 e 20/01 o retorno será: range(10/01, 14/01), range(17/01, 18/01), range(20/01, 20/01)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<DateTimeRange> GetDateRanges(this List<DateTime> source)
        {
            var ranges = new List<DateTimeRange>();

            DateTimeRange? currentRange = null;

            foreach (var date in source.OrderBy(d => d))
            {
                if (currentRange == null)
                {
                    currentRange = new DateTimeRange()
                    {
                        Date1 = date,
                        Date2 = date
                    };
                }
                else
                {
                    if (date.Date.Equals(currentRange.Value.Date2.Date.AddDays(1)))
                    {
                        currentRange = new DateTimeRange()
                        {
                            Date1 = currentRange.Value.Date1,
                            Date2 = date
                        };
                    }
                    else
                    {
                        ranges.Add(currentRange.Value);

                        currentRange = new DateTimeRange()
                        {
                            Date1 = date,
                            Date2 = date
                        };
                    }
                }
            }

            if (currentRange != null)
            {
                ranges.Add(currentRange.Value);
            }

            return ranges;
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
