namespace Hub.Infrastructure.Tasks
{
    public interface IStartupTask
    {
        void Execute();

        int Order { get; }
    }
}
