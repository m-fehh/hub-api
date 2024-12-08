namespace Hub.Shared.DataConfiguration
{
    public class TestManager
    {
        public bool RunningInTestScope { get; set; }
    }

    public enum ETestOrganizationalScope
    {
        Leaf = 1,
        Domain = 2,
        Root = 3
    }

    public class CoreTestManager
    {
        public ETestOrganizationalScope CurrentScope { get; set; }

        public long? CurrentUser { get; set; }
    }
}
