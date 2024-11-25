using Hub.Infrastructure.Database;
using System.Reflection;
using Hub.Shared.Interfaces;
using System.Collections;

namespace Hub.Infrastructure.Nominator
{
    public interface INominatorManager
    {
        /// <summary>
        /// as classes que implementarem esse método devem fornecer um nome para o objeto passado, normalmente a estratégia é usar algum marcador de propriedade para saber quais serão usadas para montar o nome.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetName(object obj);

        /// <summary>
        /// as classes que implementarem esse método devem conseguir retornar o valor da propriedade para o objeto do parametro em formato de texto
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="obj"></param>
        /// <param name="refreshIfNull"></param>
        /// <returns></returns>
        string GetPropertyDescritor(PropertyInfo prop, object obj, bool refreshIfNull = true);
    }

    public class NominatorManager : INominatorManager
    {
        public string GetName(object obj)
        {
            if (obj == null) return String.Empty;

            var namesProperties = obj.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(MainNominate), true).Length > 0);

            var name = "";

            if (namesProperties.Count() > 0)
            {
                foreach (var prop in namesProperties.OrderBy(p => ((MainNominate)p.GetCustomAttributes(typeof(MainNominate), true)[0]).Order))
                {
                    name += GetPropertyDescritor(prop, obj) + " - ";
                }

                name = name.Substring(0, name.Length - 3);
            }
            else
            {
                //tentativa de encontrar a melhor opção para representar o nome do objeto
                var keyWords = new string[] { "Name", "Description", "Code", "Key" };
                var ignoredKeywords = new string[] { "ExternalCode" };

                foreach (var key in keyWords)
                {
                    var prop = obj.GetType().GetProperties().FirstOrDefault(p => p.Name.ToUpper() == key.ToUpper());

                    if (prop != null && !ignoredKeywords.Contains(prop.Name))
                    {
                        return GetPropertyDescritor(prop, obj);
                    }
                }
                if (string.IsNullOrEmpty(name))
                {
                    foreach (var key in keyWords)
                    {
                        var prop = obj.GetType().GetProperties().FirstOrDefault(p => p.Name.ToUpper().StartsWith(key.ToUpper()));

                        if (prop != null && !ignoredKeywords.Contains(prop.Name))
                        {
                            return GetPropertyDescritor(prop, obj);
                        }
                    }
                }
                if (string.IsNullOrEmpty(name))
                {
                    foreach (var key in keyWords)
                    {
                        var prop = obj.GetType().GetProperties().FirstOrDefault(p => p.Name.ToUpper().Contains(key.ToUpper()));

                        if (prop != null && !ignoredKeywords.Contains(prop.Name))
                        {
                            return GetPropertyDescritor(prop, obj);
                        }
                    }
                }
                if (string.IsNullOrEmpty(name))
                {
                    var prop = obj.GetType().GetProperty("Id");
                    if (prop != null)
                    {
                        return GetPropertyDescritor(prop, obj);
                    }
                }
                if (string.IsNullOrEmpty(name))
                {
                    return obj.ToString();
                }
            }

            return name;
        }

        public string GetPropertyDescritor(PropertyInfo prop, object obj, bool refreshIfNull = true)
        {
            if (prop.GetMethod == null || obj == null) return String.Empty;

            var value = prop.GetValue(obj);
            if (value == null)
            {
                if (obj is IBaseEntity && refreshIfNull)
                {
                    obj = Engine.Resolve<IRepository<BaseEntity>>().RefreshAsync((BaseEntity)(obj as IBaseEntity));

                    if (obj == null) return String.Empty;

                    value = prop.GetValue(obj);
                }

                if (value == null) return String.Empty;
            }

            if (PrimitiveTypes.Test(prop.PropertyType))
            {
                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?)) return (value as DateTime?).Value.ToString("dd/MM/yyyy HH:mm:ss");

                if (prop.PropertyType == typeof(Decimal) || prop.PropertyType == typeof(Decimal?)) return (value as Decimal?).Value.ToString("0.00");

                if (prop.PropertyType == typeof(Double) || prop.PropertyType == typeof(Double?)) return (value as Double?).Value.ToString("0.00");

                if (prop.PropertyType == typeof(Boolean) || prop.PropertyType == typeof(Boolean?)) return (value as Boolean?).Value ? Engine.Get("verdadeiro") : Engine.Get("falso");

                if (prop.PropertyType.IsEnum ||
                    (Nullable.GetUnderlyingType(prop.PropertyType) != null && Nullable.GetUnderlyingType(prop.PropertyType).IsEnum)) return Engine.Get(value.ToString());

                return value.ToString();
            }
            else if (prop.PropertyType.GetInterfaces().Any(c => c.Name.StartsWith("ICollection") || (c.Name.StartsWith("IEnumerable") && c.FullName.Contains("SCH.Core.Entity"))))
            {
                var count = (value as IEnumerable).Cast<IBaseEntity>().Where(o =>
                {
                    if (o is IListItemEntity)
                    {
                        return !(o as IListItemEntity).DeleteFromList;
                    }
                    else
                    {
                        return true;
                    }
                }).Count();

                //para coleções retorno apenas a quantidade de registros
                //var count = (Int32)value.GetType().GetProperty("Count").GetValue(value);
                return count.ToString() + " " + (count <= 1 ? Engine.Get("item") : Engine.Get("itens"));
            }
            else
            {
                return GetName(value);
            }
        }
    }
}
