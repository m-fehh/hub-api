using Hub.Infrastructure.Lock.Interfaces;
using System.Linq.Expressions;

namespace Hub.Infrastructure.Builders
{
    public class CheckerItem<T> : ICheckerItem<T>
    {
        public Func<T> Func { get; }
        public Func<T, bool> IsHealthy { get; }
        public Func<bool> Condition { get; }
        public string ErrorMessage { get; }
        public ICheckerContainer Container { get; set; }

        public CheckerItem(Expression<Func<T>> funcExpression,
            Expression<Func<T, bool>> isHealthyExpression,
            Expression<Func<bool>> conditionExpression, string msgError = null)
            : this(funcExpression, isHealthyExpression, msgError)
        {
            Condition = conditionExpression.Compile();
        }

        public CheckerItem(Expression<Func<T>> funcExpression,
            Expression<Func<T, bool>> isHealthyExpression, string msgError = null)
        {
            Func = funcExpression.Compile();
            IsHealthy = isHealthyExpression.Compile();
            ErrorMessage = msgError;
        }

        /// <summary>
        /// Aplica validação
        /// </summary>
        /// <exception cref="HealthException"></exception>
        public void Validate()
        {
            var value = Func();
            if (Condition != null && !Condition())
                return;
            if (!IsHealthy(value))
                throw new HealthException(this);
        }
    }

    public class HealthCheckerResult : IHealthCheckerResult
    {
        public bool Success { get; }

        HealthCheckerResult(bool success = false)
        {
            Success = success;
        }

        private static readonly IHealthCheckerResult successInstance = new HealthCheckerResult(true);
        private static readonly IHealthCheckerResult failInstance = new HealthCheckerResult();

        public static IHealthCheckerResult Ok => successInstance;
        public static IHealthCheckerResult Failed => failInstance;
    }

    public class HealthException : Exception
    {
        public ICheckerItem CheckerItem { get; private set; }

        public HealthException(ICheckerItem checkerItem)
            : base($"{checkerItem.ErrorMessage} - {checkerItem.Container?.Father?.GetType().FullName}")
        {
            CheckerItem = checkerItem;
        }
    }
}
