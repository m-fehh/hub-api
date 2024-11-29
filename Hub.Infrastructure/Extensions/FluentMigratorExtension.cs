using FluentMigrator.Builders.Create.Table;

namespace Hub.Infrastructure.Extensions
{
    public static class FluentMigratorExtension
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax expression, string pkConstraintName)
        {
            return WithIdColumn(expression, pkConstraintName, 1, 1);
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax expression, string pkConstraintName, int identitySeed, int identityIncrement)
        {
            var column = expression.WithColumn("Id").AsInt64().NotNullable().PrimaryKey(pkConstraintName);

            return column.Identity();

        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithBinaryColumn(this ICreateTableWithColumnOrSchemaOrDescriptionSyntax expression, string name)
        {
            var column = expression.WithColumn(name);

            return column.AsBinary();

        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsMaxString(this ICreateTableColumnAsTypeSyntax expression)
        {
            return expression.AsString(int.MaxValue);
        }
    }
}
