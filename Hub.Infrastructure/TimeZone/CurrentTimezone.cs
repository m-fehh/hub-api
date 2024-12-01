using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace Hub.Infrastructure.TimeZone
{
    public interface ICurrentTimezone
    {
        TimeZoneInfo Get();

        string GetName();

        string GetServerName();

        /// <summary>
        /// Converte a data passada no timezone do cliente. Exemplo: se o servidor estiver em -03:00 e o cliente em -05:00, a data 06/11/2018 12:00 será convertida para 06/11/2018 10:00
        /// </summary>
        /// <param name="date">Data a ser convertida</param>
        /// <param name="tz"></param>
        /// <returns></returns>
        DateTime? Convert(DateTime? date, TimeZoneInfo tz = null);

        /// <summary>
        /// Converte a data passada no timezone do servidor. Exemplo: se o servidor estiver em -03:00 e o cliente em -05:00, a data 06/11/2018 10:00 será convertida para 06/11/2018 12:00
        /// </summary>
        /// <param name="date">Data a ser convertida</param>
        /// <param name="tz"></param>
        /// <returns></returns>
        DateTime? ConvertServer(DateTime? date, TimeZoneInfo tz = null);
    }

    public class CurrentTimezone : ICurrentTimezone
    {
        public TimeZoneInfo Get()
        {

            //var establishemntTimezone = Engine.Resolve<ICurrentOrganizationStructure>().GetCurrentTimezone();

            //if (establishemntTimezone != null)
            //{
            //    return establishemntTimezone;
            //}

            return TimeZoneInfo.Local;
        }

        public string GetName()
        {
            var current = Get().Id;

            if (TZConvert.TryWindowsToIana(current, out var iana))
            {
                return iana;
            }
            else
            {
                return current;
            }
        }

        public string GetServerName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TimeZoneInfo.Local.Id;
            }

            return TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);
        }

        public DateTime? Convert(DateTime? date, TimeZoneInfo tz = null)
        {
            if (date == null) return null;

            if (tz == null) tz = Get();

            if (tz == TimeZoneInfo.Local) return date;

            var utc = TimeZoneInfo.ConvertTimeToUtc(date.Value);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        public DateTime? ConvertServer(DateTime? date, TimeZoneInfo tz = null)
        {
            if (date == null) return null;

            if (tz == null) tz = Get();

            if (tz == TimeZoneInfo.Local) return date;

            var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date.Value, DateTimeKind.Unspecified), tz);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
        }
    }
}
