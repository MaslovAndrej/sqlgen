using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLQueryGen
{
    public static class AddWhereCondition
    {
        public const string OR = "OR";
        public const string AND = "AND";
    }

    public class AddWhere<T>
    {
        public IDatabase Database { get; set; }
        public string Result { get; set; }

        /*public AddWhere(string propertyName, string expression)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            var fieldName = GetQueryFieldName(mainAlias, fieldAttribute.Name);

            Result = $"{mainAlias}.{fieldAttribute.Name} {expression}";
        }*/

        public AddWhere(string propertyName, string expression, object value)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            var fieldName = GetQueryFieldName(mainAlias, fieldAttribute.Name);
            var fieldValue = Database != null ? Database.GetFieldValue(property.PropertyType, value) : value.ToString();

            if (expression == Constants.Expression.Like)
            {
                fieldName = $"lower({fieldName})";
                fieldValue = string.Format("'%{0}%'", fieldValue.Replace("'", ""));
            }

            Result = $"{mainAlias}.{fieldAttribute.Name} {expression} {fieldValue}";
        }

        private string GetQueryFieldName(string alias, string name)
        {
            return $"{alias}.{name}";
        }
    }
}
