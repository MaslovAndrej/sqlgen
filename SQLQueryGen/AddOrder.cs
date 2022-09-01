using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLQueryGen
{
    public static class AddOrderDirection
    {
        public const string ASC = "ASC";
        public const string DESC = "DESC";
    }

    public class AddOrder<T>
    {
        public string Direction { get; }
        public string Result { get; set; }

        public AddOrder(string propertyName, string direction)
        {
            Direction = direction;

            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            Result = $"{mainAlias}.{fieldAttribute.Name}";
        }
    }
}
