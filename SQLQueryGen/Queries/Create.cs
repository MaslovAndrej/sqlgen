using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen.Query
{
    internal static partial class Generator
    {
        internal static string GenerateCreateQuery<T>(IDatabase database)
        {
            if (database == null)
                return string.Empty;

            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var fieldElements = new List<string>();

            var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
            if (!properties.Any())
                return string.Empty;

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                if (fieldAttribute.Key)
                {
                    fieldElements.Add(GetQueryKeyField(fieldAttribute.Name, database.GetKeyFieldType()));
                    continue;
                }

                if (navigateAttribute != null)
                {
                    fieldElements.Add(GetQueryField(fieldAttribute.Name, database.GetNavigateFieldType(), navigateAttribute.Required));
                    continue;
                }

                var fieldType = database.GetFieldType(property.PropertyType, fieldAttribute.Size);
                fieldElements.Add(GetQueryField(fieldAttribute.Name, fieldType));
            }

            var lastFieldElement = fieldElements.Last();
            fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(database.GetCreateQuery(mainTable));
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
            queryElements.AppendLine(")");

            return queryElements.ToString();
        }

        private static string GetQueryKeyField(string name, string fieldType)
        {
            return string.Format("\"{0}\" {1} NOT NULL,", name, fieldType);
        }

        private static string GetQueryField(string name, string fieldType)
        {
            return string.Format("\"{0}\" {1},", name, fieldType);
        }

        private static string GetQueryField(string name, string fieldType, bool isRequired)
        {
            return string.Format("\"{0}\" {1} {2},", name, fieldType, isRequired ? "NOT NULL" : "NULL");
        }
    }
}
