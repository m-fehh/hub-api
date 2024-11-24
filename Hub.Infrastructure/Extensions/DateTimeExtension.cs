namespace Hub.Infrastructure.Extensions
{
    public static class DateTimeExtension
    {
        public static IEnumerable<DateTime> EachDay(this DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static IEnumerable<DateTime> EachMonth(this DateTime from, DateTime thru)
        {
            DateTime fisrtDay = new DateTime(from.Year, from.Month, 1);
            DateTime lastDay = new DateTime(thru.Year, thru.Month, 1).AddMonths(1).AddDays(-1);

            for (var day = fisrtDay.Date; day.Date <= lastDay; day = day.AddMonths(1))
                yield return day;
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime NthOf(this DateTime CurDate, int Occurrence, DayOfWeek Day)
        {
            if (Occurrence < 0)
            {
                DateTime lastDay = new DateTime(CurDate.Year, CurDate.Month, 1).AddMonths(1).AddDays(-1);
                DayOfWeek lastDow = lastDay.DayOfWeek;

                int diff = Day - lastDow;

                if (diff > 0) diff -= 7;

                return lastDay.AddDays(diff);
            }

            var fday = new DateTime(CurDate.Year, CurDate.Month, 1);

            var fOc = fday.DayOfWeek == Day ? fday : fday.AddDays(Day - fday.DayOfWeek);
            // CurDate = 2011.10.1 Occurance = 1, Day = Friday >> 2011.09.30 FIX. 
            if (fOc.Month < CurDate.Month) Occurrence = Occurrence + 1;
            return fOc.AddDays(7 * (Occurrence - 1));
        }

        /// <summary>
        /// Retorna o mês corrente a partir da data atual
        /// </summary>
        /// <param name="curDate"></param>
        /// <param name="setDate2Time">Atribui 23:59:59.999 no horário da propriedade Date2</param>
        /// <returns> Primeiro dia do mês ao ultimo dia do mês </returns>
        public static DateTimeRange CurrentMonth(this DateTime curDate, bool setDate2Time = false)
        {
            var range = new DateTimeRange()
            {
                Date1 = new DateTime(curDate.Year, curDate.Month, 1),
                Date2 = new DateTime(curDate.Year, curDate.Month, DateTime.DaysInMonth(curDate.Year, curDate.Month))
            };

            if (setDate2Time)
            {
                range.Date2 = range.Date2.AddDays(1).AddMilliseconds(-1);
            }

            return range;
        }

        /// <summary>
        /// Retorna a dezena corrente da data atual
        /// </summary>
        /// <param name="curDate"></param>
        /// <returns></returns>
        public static DateTimeRange CurrentTen(this DateTime curDate)
        {
            var offset = 0;

            if (DateTime.DaysInMonth(curDate.Year, curDate.Month) == 31)
                offset = 1;

            if (curDate.Day >= (21 + offset))
            {
                return new DateTimeRange()
                {
                    Date1 = new DateTime(curDate.Year, curDate.Month, (21 + offset)),
                    Date2 = new DateTime(curDate.Year, curDate.Month, DateTime.DaysInMonth(curDate.Year, curDate.Month))
                };
            }
            else if (curDate.Day >= (11 + offset))
            {
                return new DateTimeRange()
                {
                    Date1 = new DateTime(curDate.Year, curDate.Month, (11 + offset)),
                    Date2 = new DateTime(curDate.Year, curDate.Month, (20 + offset))
                };
            }
            else
            {
                return new DateTimeRange()
                {
                    Date1 = new DateTime(curDate.Year, curDate.Month, 1),
                    Date2 = new DateTime(curDate.Year, curDate.Month, (10 + offset))
                };
            }
        }

        public static DateTimeRange CurrentFortnight(this DateTime curDate)
        {
            var result = default(DateTimeRange);

            if (curDate.Day >= 16)
            {
                result.Date1 = new DateTime(curDate.Year, curDate.Month, 16);
                result.Date2 = new DateTime(curDate.Year, curDate.Month, DateTime.DaysInMonth(curDate.Year, curDate.Month));
                return result;
            }

            result = default(DateTimeRange);
            result.Date1 = new DateTime(curDate.Year, curDate.Month, 1);
            result.Date2 = new DateTime(curDate.Year, curDate.Month, 15);
            return result;
        }

        public static DateTimeRange CurrentWeek(this DateTime curDate)
        {
            var result = default(DateTimeRange);

            if (curDate.Day >= 24)
            {
                result = default(DateTimeRange);
                result.Date1 = new DateTime(curDate.Year, curDate.Month, 24);
                result.Date2 = new DateTime(curDate.Year, curDate.Month, DateTime.DaysInMonth(curDate.Year, curDate.Month));
                return result;
            }

            if (curDate.Day >= 16)
            {
                result = default(DateTimeRange);
                result.Date1 = new DateTime(curDate.Year, curDate.Month, 16);
                result.Date2 = new DateTime(curDate.Year, curDate.Month, 23);
                return result;
            }

            if (curDate.Day >= 8)
            {
                result = default(DateTimeRange);
                result.Date1 = new DateTime(curDate.Year, curDate.Month, 8);
                result.Date2 = new DateTime(curDate.Year, curDate.Month, 15);
                return result;
            }

            result = default(DateTimeRange);
            result.Date1 = new DateTime(curDate.Year, curDate.Month, 1);
            result.Date2 = new DateTime(curDate.Year, curDate.Month, 7);
            return result;
        }

        /// <summary>
        /// Retorna o ultimo mês(passado) referente ao dia atual
        /// </summary>
        /// <param name="curDate"></param>
        /// <returns> range do primeiro ao ultimo dia do mês passado</returns>
        public static DateTimeRange LastMonth(this DateTime curDate)
        {
            var year = curDate.Year;
            var month = curDate.Month - 1;

            if (month == 0)
            {
                month = 12;
                year = year - 1;
            }

            return new DateTimeRange()
            {
                Date1 = new DateTime(year, month, 1),
                Date2 = new DateTime(year, month, DateTime.DaysInMonth(year, month))
            };
        }

        /// <summary>
        /// Retonar range da ultima dezena a partir da data atual
        /// </summary>
        /// <param name="curDate"></param>
        /// <returns></returns>
        public static DateTimeRange LastTen(this DateTime curDate)
        {
            var offset = 0;

            if (DateTime.DaysInMonth(curDate.Year, curDate.Month) == 31)
                offset = 1;

            if (curDate.Day >= (21 + offset))
            {
                return new DateTimeRange()
                {
                    Date1 = new DateTime(curDate.Year, curDate.Month, (11 + offset)),
                    Date2 = new DateTime(curDate.Year, curDate.Month, (20 + offset))
                };
            }
            else if (curDate.Day >= (11 + offset))
            {
                return new DateTimeRange()
                {
                    Date1 = new DateTime(curDate.Year, curDate.Month, 1),
                    Date2 = new DateTime(curDate.Year, curDate.Month, (10 + offset))
                };
            }
            else
            {
                var year = curDate.Year;
                var month = curDate.Month - 1;

                if (month == 0)
                {
                    month = 12;
                    year = year - 1;
                }

                return new DateTimeRange()
                {
                    Date1 = new DateTime(year, month, (21 + offset)),
                    Date2 = new DateTime(year, month, DateTime.DaysInMonth(year, month))
                };
            }
        }

        /// <summary>
        /// Verifica se o range de datas 1 está contido no range de datas 2
        /// </summary>
        /// <param name="range"></param>
        /// <param name="range2"></param>
        /// <returns></returns>
        public static bool ViolateRange(this DateTimeRange range, DateTimeRange range2)
        {

            return ((range.Date1 >= range2.Date1 && range.Date1 < range2.Date2) ||
                    (range.Date2 > range2.Date1 && range.Date2 <= range2.Date2) ||
                    (range.Date1 <= range2.Date1 && range.Date2 >= range2.Date2));
        }

        /// <summary>
        /// Converte um número unix para um objeto data
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static int MonthDifferenceBetweenDates(this DateTime init, DateTime end)
        {
            return ((end.Year - init.Year) * 12) + end.Month - init.Month;
        }

        /// <summary>
        /// Retorna o maior valor integral menor ou igual à data especificada, considerando o intervalo desejado
        /// </summary>
        /// <param name="dateTime">Data</param>
        /// <param name="interval">Intervalo</param>
        /// <returns>Maior valor integral menor ou igual à data especificada</returns>
        public static DateTime Floor(this DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        /// <summary>
        /// Retorna o menor valor integral maior ou igual à data especificada, considerando o intervalo desejado
        /// </summary>
        /// <param name="dateTime">Data</param>
        /// <param name="interval">Intervalo</param>
        /// <returns>Menor valor integral maior ou igual à data especificada</returns>
        public static DateTime Ceiling(this DateTime dateTime, TimeSpan interval)
        {
            var overflow = dateTime.Ticks % interval.Ticks;
            return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
        }

        /// <summary>
        /// Arredonda uma data para o valor integral mais próximo considerando o intervalo informado
        /// </summary>
        /// <param name="dateTime">Data</param>
        /// <param name="interval">Intervalo</param>
        /// <returns>Data com o valor integral mais próximo considerando o intervalo informado</returns>
        public static DateTime Round(this DateTime dateTime, TimeSpan interval)
        {
            var halfIntervalTicks = (interval.Ticks + 1) >> 1;
            return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
        }

    }

    public struct DateTimeRange
    {
        public DateTime Date1 { get; set; }
        public DateTime Date2 { get; set; }
        public List<DateTime> EachDay()
        {
            return Date1.EachDay(Date2).ToList();
        }
    }
}
