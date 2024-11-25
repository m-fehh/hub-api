namespace Hub.Infrastructure.Database
{
    //public class ConnectionStringBaseConfigurator
    //{
    //    private ConnectionStringBaseVM _model;

    //    public ConnectionStringBaseConfigurator()
    //    {
    //        _model = new ConnectionStringBaseVM()
    //        {
    //            // Recupera as configurações de conexão
    //            ConnectionStringBaseSchema = Engine.AppSettings["ConnectionStringBaseSchema"],
    //            ConnectionStringNhAssembly = Engine.AppSettings["ConnectionStringNhAssembly"]
    //        };
    //    }

    //    public string GetConnectionString()
    //    {
    //        return _model?.ConnectionStringBaseSchema; 
    //    }

    //    public ConnectionStringBaseVM Get()
    //    {
    //        return _model;
    //    }

    //    public void Set(ConnectionStringBaseVM model)
    //    {
    //        _model = model;
    //    }
    //}

    //// Modelo para a string de conexão
    //public class ConnectionStringBaseVM
    //{
    //    public string ConnectionStringBaseSchema { get; set; }
    //    public string ConnectionStringNhAssembly { get; set; }
    //}

    /// <summary>
    /// Configuração da aplicação relacionada com as configurações para o NHibernate e connection string da aplicação
    /// </summary>
    public class ConnectionStringBaseConfigurator
    {
        private ConnectionStringBaseVM _model;

        public ConnectionStringBaseConfigurator()
        {
            _model = new ConnectionStringBaseVM()
            {
                ConnectionStringBaseSchema = Engine.AppSettings["ConnectionStringBaseSchema"],
                ConnectionStringNhAssembly = Engine.AppSettings["ConnectionStringNhAssembly"]
            };
        }

        public ConnectionStringBaseVM Get()
        {
            return _model;
        }

        public void Set(ConnectionStringBaseVM model)
        {
            _model = model;
        }
    }

    public class ConnectionStringBaseVM
    {
        public string ConnectionStringBaseSchema { get; set; }
        public string ConnectionStringNhAssembly { get; set; }
    }
}
