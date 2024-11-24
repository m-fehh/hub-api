namespace Hub.Infrastructure.Tasks
{
    public class StartupTask : IStartupTask
    {
        public int Order => -1;

        public void Execute()
        {
            //Singleton<Log4NetManager>.Instance = new Log4NetManager();

            //Singleton<Log4NetManager>.Instance.Configure();
        }
    }
}
