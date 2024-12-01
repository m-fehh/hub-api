using log4net.Repository.Hierarchy;

namespace Hub.Infrastructure.Logger
{
    public class Log4NetManager
    {
        Hierarchy repository;

        public void Configure()
        {
            try
            {
                repository = (Hierarchy)log4net.LogManager.CreateRepository("default");

                //repository.Root.AddAppender(new AzureTableAppender());
                //repository.Root.Level = Level.Info;

                repository.Configured = true;

                log4net.Config.BasicConfigurator.Configure(repository);
            }
            catch (Exception)
            {
            }
        }

        public void Info(string loggerName, object log)
        {
            log4net.LogManager.GetLogger("default", loggerName).Info(log);
        }

        public void Warn(string loggerName, object log)
        {
            log4net.LogManager.GetLogger("default", loggerName).Warn(log);
        }

        public void Error(string loggerName, object log, Exception ex = null)
        {
            log4net.LogManager.GetLogger("default", loggerName).Error(log, ex);
        }
    }
}
