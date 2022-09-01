using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen.Query
{
    internal static partial class Generator
    {
        internal static string GenerateDeleteQuery<T>(IDatabase database)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            return database.GetDeleteQuery(mainTable);
        }

        internal static string GenerateDeleteQuery<T>(IDatabase database, AddWhere<T> addWhere)
        {
            addWhere.Database = database;

            var query = GenerateDeleteQuery<T>(database);

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        internal static string GenerateDeleteQuery<T>(IDatabase database, List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateDeleteQuery<T>(database);

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            foreach (var addWhere in addWhereList)
            {
                addWhere.Database = database;
                whereElements.Add(addWhere.Result);
                whereElements.Add(addWhereCondition);
            }
            whereElements.RemoveAt(whereElements.Count - 1);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateDeleteQuery<T>(IDatabase database, T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var property = type.GetProperties()
              .Where(x => x.GetCustomAttributes()
                .Where(xx => xx is FieldAttribute)
                .Cast<FieldAttribute>()
                .Any(xxx => xxx.Key)).FirstOrDefault();

            if (property == null)
                throw new Exception("Deletion is possible only if there is a key property.");

            var value = property.GetValue(entity);
            if (value == null)
                throw new Exception("Deletion is possible only if there is a value of the key property.");

            var keyField = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault().Name;
            var keyValue = database.GetFieldValue(property.PropertyType, value);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(GenerateDeleteQuery<T>(database));
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine($"{keyField} = {keyValue}");

            return queryElements.ToString();
        }
    }
}
