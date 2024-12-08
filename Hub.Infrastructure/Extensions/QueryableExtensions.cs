//using System.Linq.Expressions;

//namespace Hub.Infrastructure.Extensions
//{
//    public static class QueryableExtensions
//    {
//        /// <summary>
//        /// Aplica ordenação a uma consulta IQueryable.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="sortExpression">Expressão de ordenação</param>
//        /// <param name="ascending">Indica se a ordenação será crescente (padrão) ou decrescente</param>
//        /// <returns>Consulta ordenada</returns>
//        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, Expression<Func<T, object>> sortExpression, bool ascending = true)
//        {
//            if (sortExpression == null)
//                return query;

//            return ascending ? query.OrderBy(sortExpression) : query.OrderByDescending(sortExpression);
//        }

//        /// <summary>
//        /// Aplica paginação a uma consulta IQueryable.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="page">Número da página</param>
//        /// <param name="pageSize">Número de elementos por página</param>
//        /// <returns>Consulta paginada</returns>
//        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
//        {
//            if (page < 1)
//                page = 1;
//            if (pageSize < 1)
//                pageSize = 10;

//            return query.Skip((page - 1) * pageSize).Take(pageSize);
//        }

//        /// <summary>
//        /// Aplica filtros baseados em um dicionário de chaves e valores a uma consulta IQueryable.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="filters">Dicionário de filtros chave-valor</param>
//        /// <returns>Consulta filtrada</returns>
//        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, IDictionary<string, object> filters)
//        {
//            if (filters == null || !filters.Any())
//                return query;

//            foreach (var filter in filters)
//            {
//                var propertyInfo = typeof(T).GetProperty(filter.Key);
//                if (propertyInfo == null) continue;

//                var parameter = Expression.Parameter(typeof(T), "x");
//                var member = Expression.Property(parameter, propertyInfo);
//                var constant = Expression.Constant(filter.Value);
//                var equals = Expression.Equal(member, constant);

//                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
//                query = query.Where(lambda);
//            }

//            return query;
//        }

//        /// <summary>
//        /// Conta o número de elementos de uma consulta IQueryable de forma síncrona.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <returns>Quantidade de elementos</returns>
//        public static int Count<T>(this IQueryable<T> query)
//        {
//            return query.Count();
//        }

//        /// <summary>
//        /// Conta o número de elementos de uma consulta IQueryable de forma assíncrona.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <returns>Quantidade de elementos</returns>
//        public static async Task<int> CountAsync<T>(this IQueryable<T> query)
//        {
//            return await query.CountAsync();
//        }

//        /// <summary>
//        /// Conta o número de elementos de uma consulta IQueryable de forma assíncrona, com um filtro.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="predicate">Expressão de filtro</param>
//        /// <returns>Quantidade de elementos que atendem ao filtro</returns>
//        public static async Task<int> CountAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
//        {
//            return await query.Where(predicate).CountAsync();
//        }

//        /// <summary>
//        /// Projeta os elementos de uma consulta IQueryable para um tipo diferente.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento original em IQueryable</typeparam>
//        /// <typeparam name="TResult">Tipo do elemento projetado</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="selector">Expressão de projeção</param>
//        /// <returns>Consulta projetada</returns>
//        public static IQueryable<TResult> ProjectTo<T, TResult>(this IQueryable<T> query, Expression<Func<T, TResult>> selector)
//        {
//            return query.Select(selector);
//        }

//        /// <summary>
//        /// Verifica se a consulta IQueryable contém elementos.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <returns>True se contiver elementos, caso contrário False</returns>
//        public static bool Any<T>(this IQueryable<T> query)
//        {
//            return query.Any();
//        }

//        /// <summary>
//        /// Verifica se a consulta IQueryable contém elementos que atendem ao critério especificado.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="predicate">Expressão de filtro</param>
//        /// <returns>True se houver elementos que atendem ao critério, caso contrário False</returns>
//        public static bool Any<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
//        {
//            return query.Any(predicate);
//        }

//        /// <summary>
//        /// Converte uma consulta IQueryable para uma lista de forma assíncrona.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <returns>Lista de elementos</returns>
//        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
//        {
//            return await query.ToListAsync();
//        }

//        /// <summary>
//        /// Retorna o primeiro elemento ou o valor padrão de uma consulta IQueryable de forma assíncrona.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <returns>Primeiro elemento ou valor padrão</returns>
//        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> query)
//        {
//            return await query.FirstOrDefaultAsync();
//        }

//        /// <summary>
//        /// Retorna o primeiro elemento que atende ao critério ou o valor padrão de uma consulta IQueryable de forma assíncrona.
//        /// </summary>
//        /// <typeparam name="T">Tipo do elemento em IQueryable</typeparam>
//        /// <param name="query">Consulta IQueryable</param>
//        /// <param name="predicate">Expressão de filtro</param>
//        /// <returns>Primeiro elemento que atende ao critério ou valor padrão</returns>
//        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
//        {
//            return await query.FirstOrDefaultAsync(predicate);
//        }
//    }
//}
