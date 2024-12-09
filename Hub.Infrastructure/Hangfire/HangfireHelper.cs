using Hangfire.Annotations;
using Hangfire.Server;
using Hangfire.Storage.Monitoring;
using Hangfire;
using System.Linq.Expressions;
using Hangfire.Console;

namespace Hub.Infrastructure.Hangfire
{
    public interface IHangfireHelper
    {
        string Enqueue([InstantHandle, NotNull] Expression<Action> methodCall);
        string Enqueue([InstantHandle, NotNull] Expression<Func<Task>> methodCall);
        string Schedule([InstantHandle, NotNull] Expression<Action> methodCall, DateTimeOffset enqueueAt);
        JobDetailsDto GetJobDetails(string jobId);
    }

    public class HangfireHelper : IHangfireHelper
    {
        public string Enqueue([InstantHandle, NotNull] Expression<Action> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public string Enqueue([InstantHandle, NotNull] Expression<Func<Task>> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public string Schedule([InstantHandle, NotNull] Expression<Action> methodCall, DateTimeOffset enqueueAt)
        {
            return BackgroundJob.Schedule(methodCall, enqueueAt);
        }

        public JobDetailsDto GetJobDetails(string jobId)
        {
            return JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
        }
    }

    public static class HangfireAction
    {
        /// <summary>
        /// Registra uma mensagem no log do hangfire
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Log(PerformContext context, string type, string message)
        {
            switch (type)
            {
                case "info":
                    context.WriteLine(ConsoleTextColor.Cyan, message);
                    break;
                case "error":
                    context.WriteLine(ConsoleTextColor.Red, message);
                    break;
                default:
                    context.WriteLine(message);
                    break;
            }
        }
    }
}
