using Hub.Infrastructure.Autofac;

namespace Hub.Infrastructure.Search
{
    public static class SearchBootstrapper
    {
        /// <summary>
        /// Esse inicializador faz a busca dentro de todos os assemblies do sistema por classes que implementam a interface <see cref="ISearchItem"/> e os registra numa coleção (<see cref="SearchCollection"/>) para facilitar o acesso.
        /// </summary>
        public static void Initialize(List<ISearchItem> itens)
        {
            var collection = new SearchCollection();
            collection.Itens.AddRange(itens);

            Singleton<SearchCollection>.Instance = collection;
        }
    }
}
