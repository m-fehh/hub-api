namespace Hub.Infrastructure.Database.NhManagement
{
    public static class NhGlobalData
    {
        public const string QueryHintMaxdopCommentString = "queryhint-option-maxdop";
        public const string QueryHintNoLock = "queryhint-nolock";

        public static Action CloseCurrentSession { get; set; }

        public static Action CloseCurrentFactory { get; set; }

        public static void StartSessionFactory()
        {
            NhSessionProvider.GetSessionFactory();
        }
    }
}
