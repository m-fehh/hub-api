namespace Hub.Infrastructure.Search
{
    /// <summary>
    /// Classe que guarda uma coleção de todas as classes que implementam o <see cref="ISearchItem"/>.
    /// </summary>
    public class SearchCollection
    {
        public SearchCollection()
        {
            Itens = new List<ISearchItem>();
        }

        public List<ISearchItem> Itens { get; set; }
    }
}
