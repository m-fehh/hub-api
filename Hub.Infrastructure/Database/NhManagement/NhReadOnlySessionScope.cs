using Hub.Infrastructure.Extensions;

namespace Hub.Infrastructure.Database.NhManagement
{
    /// <summary>
    /// Interface usada apenas para controlar através de injeção de dependencia quando a fábrica de sessões deve fornecer a conexão read-only do banco de dados
    /// </summary>
    public interface INhReadOnlySessionScope : IDisposable
    {
        public INhReadOnlySessionScope Start();

        public ESessionMode GetCurrentSessionMode();
    }

    /// <summary>
    /// Implementa um controle de escopo da aplicação para definir para o provedor de sessões do NHibernate de que deve-se obter a conexão de uma connectionString readOnly
    /// </summary>
    public class NhReadOnlySessionScope : INhReadOnlySessionScope
    {
        public INhReadOnlySessionScope Start()
        {
            if (NhSessionProvider.SessionMode.Value == ESessionMode.ReadOnly)
            {
                throw new BusinessException("A sessão já estava em modo somente leitura. Não é permitido executar um sub-escopo.");
            }

            NhSessionProvider.SessionMode.Value = ESessionMode.ReadOnly;

            //existem threads reaproveitadas que deixam a sessão anterior em aberto. Aqui forçamos o encerramento de qualquer sessão que possa ter ficado aberta por outra thread.
            if (NhGlobalData.CloseCurrentSession != null) NhGlobalData.CloseCurrentSession();

            return this;
        }

        public ESessionMode GetCurrentSessionMode()
        {
            return NhSessionProvider.SessionMode.Value;
        }

        public void Dispose()
        {
            NhSessionProvider.SessionMode.Value = ESessionMode.Normal;
        }
    }
}
