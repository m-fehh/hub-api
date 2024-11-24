namespace Hub.Infrastructure.Autofac
{
    public class Singleton<T> : Singleton
    {
        static T instance;

        /// <summary>
        /// Instância única do objeto do tipo T. Apenas uma instância para o tipo T pode ser armazenada.
        /// </summary>
        public static T Instance
        {
            get { return instance; }
            set
            {
                instance = value;
                AllSingletons[typeof(T)] = value;
            }
        }
    }

    public class Singleton
    {
        static Singleton()
        {
            allSingletons = new Dictionary<Type, object>();
        }

        static readonly IDictionary<Type, object> allSingletons;

        /// <summary>
        /// Dicionário de tipos que contem as instâncias dos objetos singletons
        /// </summary>
        public static IDictionary<Type, object> AllSingletons
        {
            get { return allSingletons; }
        }
    }
}
