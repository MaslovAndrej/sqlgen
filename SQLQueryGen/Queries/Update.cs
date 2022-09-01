using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen.Query
{
    internal static partial class Generator
    {
        internal static string GenerateUpdateQuery<T>(IDatabase database, T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var keyValue = string.Empty;
            var updateElements = new List<string>();

            var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                if (fieldAttribute.Key)
                {
                    keyField = fieldAttribute.Name;
                    keyValue = database.GetFieldValue(property.PropertyType, property.GetValue(entity));
                    continue;
                }

                var value = property.GetValue(entity);
                if (value == null)
                    continue;

                if (navigateAttribute != null)
                {
                    var navigateProperty = value.GetType().GetProperties()
                      .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute && (xx as FieldAttribute).Name == navigateAttribute.FieldName))
                      .FirstOrDefault();
                    var navigatePropertyValue = navigateProperty.GetValue(value);
                    if (navigatePropertyValue == null)
                        continue;

                    value = navigatePropertyValue;
                }

                var updateValue = database.GetFieldValue(property.PropertyType, value);
                updateElements.Add(GetQueryFieldValue(fieldAttribute.Name, updateValue));
            }

            var lastUpdateElement = updateElements.Last();
            updateElements[updateElements.Count - 1] = lastUpdateElement.Substring(0, lastUpdateElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(database.GetUpdateQuery(mainTable));
            queryElements.AppendLine("SET");
            queryElements.AppendLine(string.Join(Environment.NewLine, updateElements));
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine(GetQueryKeyFieldValue(keyField, keyValue));

            return queryElements.ToString();
        }

        private static string GetQueryKeyFieldValue(string name, string value)
        {
            return $"\"{name}\" = \"{value}\"";
        }

        private static string GetQueryFieldValue(string name, string value)
        {
            return $"\"{name}\" = \"{value}\",";
        }
    }
}
