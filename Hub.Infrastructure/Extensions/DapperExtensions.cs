using System.Data;
using Dapper;

namespace Hub.Infrastructure.Extensions
{
    public static class DapperExtensions
    {
        /// <summary>
        /// permite passar uma função para criar um tipo anonimo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="typeBuilder"></param>
        /// <param name="sql"></param>
        /// <example>
        /// var data = connection.Query(() => new { ContactId = default(int), Name = default(string) }, "SELECT ContactId, Name FROM Contact");
        /// </example>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.Query<T>(sql);
        }

        public static T QueryFirst<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.QueryFirst<T>(sql);
        }

        public static T QueryFirstOrDefault<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.QueryFirstOrDefault<T>(sql);
        }

        public static T QuerySingle<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.QuerySingle<T>(sql);
        }

        public static T QuerySingleOrDefault<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.QuerySingleOrDefault<T>(sql);
        }
    }
}
