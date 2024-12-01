using Dapper;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Shared.DataConfiguration.Admin;
using Hub.Shared.Models.VMs;
using Microsoft.Data.SqlClient;

namespace Hub.Application.Services.Admin
{
    public class TenantService : CrudService<Tenants>
    {
        private readonly string _connectionString;

        public TenantService(IRepository<Tenants> repository) : base(repository)
        {
            _connectionString = Engine.ConnectionString("adm") ?? throw new InvalidOperationException("Connection string 'adm' not found.");
        }

        private void Validate(TenantVM entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "The tenant entity cannot be null.");

            using (var connection = new SqlConnection(_connectionString))
            {
                const string validateQuery = @"
                    SELECT COUNT(1) 
                    FROM adm.tenants
                    WHERE Name = @Name OR Subdomain = @Subdomain";

                var exists = connection.ExecuteScalar<int>(validateQuery, new
                {
                    entity.Name,
                    entity.Subdomain
                });

                if (exists > 0)
                {
                    throw new InvalidOperationException("A tenant with the same Name or Subdomain already exists.");
                }
            }
        }

        private void ValidateInsert(TenantVM entity)
        {
            Validate(entity);
        }

        public long Insert(TenantVM entity)
        {
            ValidateInsert(entity);

            using (var connection = new SqlConnection(_connectionString))
            {
                const string insertQuery = @"
                    INSERT INTO adm.tenants (Name, Subdomain, IsActive, CultureName)
                    VALUES (@Name, @Subdomain, @IsActive, @CultureName);
                    SELECT CAST(SCOPE_IDENTITY() as bigint);";

                var insertedId = connection.ExecuteScalar<long>(insertQuery, new
                {
                    entity.Name,
                    entity.Subdomain,
                    entity.IsActive,
                    entity.CultureName
                });

                return insertedId;
            }
        }
    }
}
