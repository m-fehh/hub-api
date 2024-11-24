namespace Hub.Infrastructure.MultiTenant
{
    // <summary>
    /// Armazena o nome do tenant que deve ser utilizado na aplicação durante o escopo criado
    /// </summary>
    public class TenantLifeTimeScope : IDisposable
    {
        public string CurrentTenantName { get; set; }

        public IDisposable Start(string CurrentTenantName)
        {
            this.CurrentTenantName = CurrentTenantName;

            return this;
        }

        public void Dispose()
        {
            this.CurrentTenantName = null;
        }
    }
}
