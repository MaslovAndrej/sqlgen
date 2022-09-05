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
        private string Property { get; set; }
        private string Expression { get; set; }
        private object Value { get; set; }
        public string Result { get; set; }

        public AddWhere(string propertyName, string expression, object value)
        {
            Property = propertyName;
            Expression = expression;
            Value = value;
        }

        internal AddWhere(IDatabase database, AddWhere<T> addWhere)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == addWhere.Property).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            var fieldName = GetQueryFieldName(mainAlias, fieldAttribute.Name);
            var fieldValue = database != null ? database.GetFieldValue(property.PropertyType, addWhere.Value) : addWhere.Value.ToString();

            if (addWhere.Expression == Constants.Expression.Like)
            {
                fieldName = $"lower({fieldName})";
                fieldValue = string.Format("'%{0}%'", fieldValue.Replace("'", ""));
            }

            Result = $"{mainAlias}.{fieldAttribute.Name} {addWhere.Expression} {fieldValue}";
        }

        private string GetQueryFieldName(string alias, string name)
        {
            return $"{alias}.{name}";
        }
    }
}
