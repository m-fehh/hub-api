namespace Hub.Infrastructure.Database.NhManagement
{
    public interface INhStartSessionFactory
    {
        Action PopulateTenantCollection { get; set; }

        void InitSessionFactory(string tenantName);
    }

    public class NhStartSessionFactory : INhStartSessionFactory
    {
        public Action PopulateTenantCollection { get; set; }

        /// <summary>
        /// chama o método de criação da fábrica de sessão para que a mesma seja inicializada
        /// </summary>
        /// <param name="tenantName"></param>
        public void InitSessionFactory(string tenantName)
        {
            NhSessionProvider.GetSessionFactory(tenantName);
        }
    }
}
