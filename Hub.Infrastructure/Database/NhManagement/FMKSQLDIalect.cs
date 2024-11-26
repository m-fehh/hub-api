using NHibernate;
using NHibernate.Dialect.Function;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using NHibernate.Util;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Hub.Infrastructure.Database.NhManagement
{
    class FMKSQLDIalect : NHibernate.Dialect.MsSqlAzure2008Dialect
    {
        public FMKSQLDIalect()
        {
            RegisterFunction("date", new SQLFunctionTemplate(NHibernateUtil.Date, "cast(?1 as date)"));

            RegisterFunction("initmonth", new SQLFunctionTemplate(NHibernateUtil.Date, "DATEFROMPARTS(year(?1), month(?1), 1)"));

            RegisterFunction("datetime", new SQLFunctionTemplate(NHibernateUtil.Date, "DATETIMEFROMPARTS(datepart(year, ?1),datepart(month, ?1),datepart(day, ?1),datepart(hour, ?1),datepart(minute, ?1), 0, 0)"));

            RegisterFunction("datetimestring", new SQLFunctionTemplate(NHibernateUtil.Date, "Convert(varchar(16), ?1, 121)"));

            RegisterFunction("AddDays", new SQLFunctionTemplate(NHibernateUtil.DateTime, "dateadd(day,?2,?1)"));

            RegisterFunction("AddHours", new SQLFunctionTemplate(NHibernateUtil.DateTime, "dateadd(hour,?2,?1)"));

            RegisterFunction("AddMinutes", new SQLFunctionTemplate(NHibernateUtil.DateTime, "dateadd(minute,?2,?1)"));
        }
    }

    public class ExtendedLinqtoHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public ExtendedLinqtoHqlGeneratorsRegistry()
        {
            this.Merge(new AddDaysGenerator());
            this.Merge(new AddHoursGenerator());
            this.Merge(new AddMinutesGenerator());
        }
    }

    public class AddDaysGenerator : BaseHqlGeneratorForMethod
    {
        public AddDaysGenerator()
        {
            SupportedMethods = new[]
            {
                ReflectHelper.GetMethodDefinition<DateTime?>(d => d.Value.AddDays((double) 0))
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            return treeBuilder.MethodCall("AddDays", visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
        }
    }
    public class AddHoursGenerator : BaseHqlGeneratorForMethod
    {
        public AddHoursGenerator()
        {
            SupportedMethods = new[]
            {
                ReflectHelper.GetMethodDefinition<DateTime?>(d => d.Value.AddHours((double) 0))
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            return treeBuilder.MethodCall("AddHours", visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
        }
    }

    public class AddMinutesGenerator : BaseHqlGeneratorForMethod
    {
        public AddMinutesGenerator()
        {
            SupportedMethods = new[]
                {
                ReflectionHelper.GetMethodDefinition<DateTime?>(d => d.Value.AddMinutes((double) 0))
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            return treeBuilder.MethodCall("AddMinutes", visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
        }
    }
}
