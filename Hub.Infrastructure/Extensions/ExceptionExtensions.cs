using System.Text;

namespace Hub.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Monta um texto para exibição de uma exception
        /// </summary>
        public static string CreateExceptionString(this Exception e)
        {
            StringBuilder sb = new StringBuilder();
            CreateExceptionString(sb, e, string.Empty);

            return sb.ToString();
        }

        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent)
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Sub-", indent);
            }

            sb.AppendFormat("Erro Encontrado:\n{0}Type: {1}", indent, e.GetType().FullName);
            sb.AppendFormat("\n{0}Mensagem: {1}", indent, e.Message);
            sb.AppendFormat("\n{0}Fonte: {1}", indent, e.Source);
            sb.AppendFormat("\n{0}Stacktrace: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                sb.Append("\n");
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }
        }
    }
}
