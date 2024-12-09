using FluentMigrator;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Extensions;

namespace Hub.Infrastructure.Database.NhManagement
{
    public class DataReferenceMap
    {
        public string ForeignKeyName { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    public class DataReferenceMapManager
    {
        public List<DataReferenceMap> DataReferenceMaps { get; set; }

        public DataReferenceMapManager()
        {
            DataReferenceMaps = new List<DataReferenceMap>();
        }

        private static DataReferenceMapManager _current;
        public static DataReferenceMapManager Current
        {
            get { return _current ?? (_current = new DataReferenceMapManager()); }
        }

        /// <summary>
        /// Analisa a exception para verificar se a origem foi uma das FKs mapeadas pela coleção <see cref="DataReferenceMap"/>. Se for, gera uma exception tratada.
        /// </summary>
        /// <param name="ex">Exceção a ser analisada para tratar em caso de FK</param>
        public static void FindFKException(Exception ex)
        {
            foreach (var item in Current.DataReferenceMaps)
            {
                if (ex.Message.ToUpper().Contains(item.ForeignKeyName.ToUpper()))
                {
                    throw new BusinessException(string.Format(Engine.Get("excecao_tratada_fk"), Engine.Get(item.Destination), Engine.Get(item.Source)));
                }
            }

            if (ex.InnerException != null)
            {
                FindFKException(ex.InnerException);
            }

            throw ex;
        }

        /// <summary>
        /// Cria a foreign key no banco de dados através da API Fluent Migrator (quando o migrator for diferente de null) ou insere a referencia da foreign key na coleção da classe (migrator = null) <see cref="DataReferenceMapManager"/>
        /// </summary>
        /// <param name="migration"></param>
        /// <param name="schema"></param>
        /// <param name="foreignKey">nome da foreign key</param>
        /// <param name="source">tabela de origem da foreign key</param>
        /// <param name="columnSource">coluna da tabela de origem</param>
        /// <param name="destination">tabela de destino da foreign key</param>
        /// <param name="columnDestination">coluna da tabela de destino</param>
        /// <param name="translateSource">tradução para a coleção da classe <see cref="DataReferenceMapManager"/>. Quando em branco, o sistema tentará usar a própria tabela como fonte de tradução.</param>
        /// <param name="translateDestination">tradução para a coleção da classe <see cref="DataReferenceMapManager"/>. Quando em branco, o sistema tentará usar a própria tabela como fonte de tradução.</param>
        /// <param name="destinationSchema">Schema de destino caso a tabela a ser referenciada se encontre em um schema diferente.</param>
        public static void CreateForeignKey(
            MigrationBase migration,
            string schema,
            string foreignKey,
            string source,
            string columnSource,
            string destination,
            string columnDestination = "Id",
            string translateSource = null,
            string translateDestination = null,
            string destinationSchema = null)
        {
            if (migration != null)
            {
                if (!string.IsNullOrWhiteSpace(destinationSchema))
                    migration.Create.ForeignKey(foreignKey).FromTable(source).InSchema(schema).ForeignColumn(columnSource).ToTable(destination).InSchema(destinationSchema).PrimaryColumn(columnDestination);
                else
                    migration.Create.ForeignKey(foreignKey).FromTable(source).InSchema(schema).ForeignColumn(columnSource).ToTable(destination).InSchema(schema).PrimaryColumn(columnDestination);
            }
            else
            {
                if (translateSource == null)
                {
                    translateSource = source;
                }

                if (translateDestination == null)
                {
                    translateDestination = destination;
                }

                DataReferenceMapManager.Current.DataReferenceMaps.Add(new DataReferenceMap()
                {
                    ForeignKeyName = foreignKey,
                    Source = translateSource,
                    Destination = translateDestination
                });
            }
        }
    }
}
