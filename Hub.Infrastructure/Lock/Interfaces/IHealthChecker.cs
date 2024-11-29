using Hub.Infrastructure.Builders;

namespace Hub.Infrastructure.Lock.Interfaces
{
    public interface IHealthChecker
    {
        CheckerContainer CheckerContainer { get; }
    }

    public interface IHealthCheckerResult
    {
        bool Success { get; }
    }
}
