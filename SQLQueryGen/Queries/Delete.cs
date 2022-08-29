using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static class QueryGenerator
    {


        #region GenerateDelete.

        public static string GenerateDeleteQuery<T>()
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            return string.Format("DELETE FROM {0}.{1} {2}", DBInitializer.Schema, mainTable, mainAlias);
        }

        public static string GenerateDeleteQuery<T>(AddWhere<T> addWhere)
        {
            var query = GenerateDeleteQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateDeleteQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateDeleteQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            foreach (var addWhere in addWhereList)
            {
                whereElements.Add(addWhere.Result);
                whereElements.Add(addWhereCondition);
            }
            whereElements.RemoveAt(whereElements.Count - 1);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateDeleteQuery<T>(T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var property = type.GetProperties()
              .Where(x => x.GetCustomAttributes()
                .Where(xx => xx is FieldAttribute)
                .Cast<FieldAttribute>()
                .Any(xxx => xxx.Key)).FirstOrDefault();

            if (property == null)
                throw new Exception("Удаление возможно только при наличии ключевого свойства.");

            var value = property.GetValue(entity);
            if (value == null)
                throw new Exception("Удаление возможно только при наличии значения ключевого свойства.");

            var keyField = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault().Name;
            var keyValue = GetFieldValue(property.PropertyType, value);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(string.Format("DELETE FROM {0}.{1}", DBInitializer.Schema, mainTable));
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine(string.Format("{0} = {1}", keyField, keyValue));

            return queryElements.ToString();
        }

        #endregion


    }

}
