using FluentMigrator.Builders.Execute;

namespace Hub.Domain.Database.Helpers
{
    public static class AddExtendedPropertyHelper
    {
        public static void UpdateExtendedProperty(this IExecuteExpressionRoot execute, string schema, ExtendedPropertyParameter parameter)
        {
            ExtendedProperty(execute, schema, parameter, false);
        }

        public static void AddExtendedProperty(this IExecuteExpressionRoot execute, string schema, ExtendedPropertyParameter parameter)
        {
            ExtendedProperty(execute, schema, parameter);
        }

        private static void ExtendedProperty(IExecuteExpressionRoot execute, string schema, ExtendedPropertyParameter parameter, bool createTable = true)
        {
            if (parameter != null)
            {
                if (parameter.IsReady)
                {
                    if (createTable)
                    {
                        execute.Sql(@$"
                        EXEC sys.sp_addextendedproperty
                            @name=N'Descrição da Tabela',
                            @value=N'{parameter.TableDescription}' , 
                            @level0type=N'SCHEMA',
                            @level0name=N'{schema}', 
                            @level1type=N'TABLE',
                            @level1name=N'{parameter.TableName}' 
                        GO");
                    }

                    foreach (var column in parameter.Columns)
                    {
                        execute.Sql(@$"
                            EXEC sys.sp_addextendedproperty
                                @name=N'Descrição da Coluna',
                                @value=N'{column.ColumnDescription}',
                                @level0type=N'SCHEMA',
                                @level0name=N'{schema}',
                                @level1type=N'TABLE', 
                                @level1name=N'{parameter.TableName}', 
                                @level2type=N'COLUMN', 
                                @level2name=N'{column.ColumnName}'
                            GO");
                    }
                }
            }
        }
    }

    public class ExtendedPropertyParameter
    {
        public ExtendedPropertyParameter()
        {
            Columns = new List<ExtendedPropertyColumnParameter>();
        }

        public string TableName { get; set; }
        public string TableDescription { get; set; }
        public IEnumerable<ExtendedPropertyColumnParameter> Columns { get; set; }

        public bool IsReady
        {
            get
            {
                return !string.IsNullOrEmpty(TableName)
                    && !string.IsNullOrEmpty(TableDescription)
                    && Columns.Any();
            }
        }
    }

    public class ExtendedPropertyColumnParameter
    {
        public string ColumnName { get; set; }
        public string ColumnDescription { get; set; }
    }
}
