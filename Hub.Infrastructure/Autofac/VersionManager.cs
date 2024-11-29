using System.Reflection;

namespace Hub.Infrastructure.Autofac
{
    public interface IVersionManager
    {
        string GetVersion();
    }

    public class VersionManager : IVersionManager
    {
        public string GetVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            return new DateTime(2000, 1, 1).Add(new TimeSpan(TimeSpan.TicksPerDay * version.Build + TimeSpan.TicksPerSecond * 2 * version.Revision)).ToString("yy.MM.dd.HH.mm");
        }
    }
}
