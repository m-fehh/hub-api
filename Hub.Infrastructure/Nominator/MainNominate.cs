namespace Hub.Infrastructure.Nominator
{
    /// <summary>
    /// Classe que define as propriedade que nomearão o objeto
    /// </summary>
    public class MainNominate : Attribute
    {
        /// <summary>
        /// Caso uma classe possua mais de uma propriedade com esse atributo, então a ordem é que definirá como o nome será montado
        /// </summary>
        public int Order { get; set; }
    }
}
