using NHibernate.Classic;

namespace Hub.Infrastructure.Builders
{
    public interface ICheckerContainer : IValidable
    {
        object Father { get; set; }
        List<ICheckerItem> Items { get; set; }
    }

    public interface ICheckerItem : IValidable
    {
        ICheckerContainer Container { get; set; }
        string ErrorMessage { get; }
    }

    public interface ICheckerItem<T> : ICheckerItem
    {
        Func<T> Func { get; }
        Func<T, bool> IsHealthy { get; }
        Func<bool> Condition { get; }
    }

    public interface IValidable<T>
    {
        void Validate(T value);
    }

    public interface IValidable
    {
        void Validate();
    }

    public class CheckerContainer : ICheckerContainer
    {
        public object Father { get; set; }
        public List<ICheckerItem> Items { get; set; } = new List<ICheckerItem>();

        public CheckerContainer()
        {
        }

        public CheckerContainer(object father)
        {
            Father = father;
        }

        public void Validate()
        {
            foreach (var item in Items)
            {
                item.Validate();
            }
        }
    }
}
