using NHibernate;
using NHibernate.SqlCommand;
using System.Text.RegularExpressions;

namespace Hub.Infrastructure.Database.NhManagement
{
    [Serializable]
    public class QueryHintInterceptor : EmptyInterceptor
    {
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            if (sql.ToString().Contains(string.Concat("/* ", NhGlobalData.QueryHintMaxdopCommentString)))
            {
                var maxdop = Engine.AppSettings["QueryMaxdop"] ?? "6";
                string pattern = $@"\/\* {NhGlobalData.QueryHintMaxdopCommentString}\((\d+)\)";

                Match match = Regex.Match(sql.ToString(), pattern);
                if (match.Success)
                {
                    maxdop = match.Groups[1].Value;
                }

                return sql.Insert(sql.Length, $" option(MAXDOP {maxdop})");
            }

            if (sql.ToString().Contains($"/* {NhGlobalData.QueryHintNoLock}"))
            {
                // Modify the sql to add hints
                if (sql.IndexOfCaseInsensitive("select") > 0)
                {
                    var parts = sql.ToString().Split().ToList();
                    var fromItem = parts.FirstOrDefault(p => p.Trim().Equals("from", StringComparison.OrdinalIgnoreCase));
                    int fromIndex = fromItem != null ? parts.IndexOf(fromItem) : -1;
                    var whereItem = parts.FirstOrDefault(p => p.Trim().Equals("where", StringComparison.OrdinalIgnoreCase));
                    int whereIndex = whereItem != null ? parts.IndexOf(whereItem) : parts.Count;
                    if (fromIndex == -1)
                        return sql;
                    parts.Insert(parts.IndexOf(fromItem) + 3, "WITH (NOLOCK)");
                    for (int i = fromIndex; i < whereIndex; i++)
                    {
                        if (parts[i - 1].Equals(","))
                        {
                            parts.Insert(i + 3, "WITH (NOLOCK)");
                            i += 3;
                        }
                        if (parts[i].Trim().Equals("on", StringComparison.OrdinalIgnoreCase))
                        {
                            parts[i] = "WITH (NOLOCK) on";
                        }
                    }
                    // MUST use SqlString.Parse() method instead of new SqlString()
                    sql = SqlString.Parse(string.Join(" ", parts));
                }
            }

            return base.OnPrepareStatement(sql);
        }
    }
}
