namespace Hub.Infrastructure.Builders
{
    public class CheckerContainerBuilder
    {
        private CheckerContainer _instance = new CheckerContainer();

        public CheckerContainerBuilder(object father)
        {
            _instance.Father = father;
        }

        public CheckerContainerBuilder AddItem(ICheckerItem item)
        {
            item.Container = _instance;
            _instance.Items.Add(item);
            return this;
        }

        public CheckerContainer Build()
        {
            return _instance;
        }
    }
}
