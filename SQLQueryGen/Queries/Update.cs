using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static partial class Generator
    {
        public static string GenerateUpdateQuery<T>(DBInstance instance, T entity)
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
                    var propValue = property.GetValue(entity);
                    var isKeyNumeric = property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double);
                    keyValue = string.Format(isKeyNumeric ? "{0}" : "'{0}'", isKeyNumeric ? propValue.ToString().Replace(",", ".") : propValue.ToString());
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

                var updateValue = instance.GetFieldValue(property.PropertyType, value);
                updateElements.Add(string.Format("{0} = {1},", fieldAttribute.Name, updateValue));
            }

            var lastUpdateElement = updateElements.Last();
            updateElements[updateElements.Count - 1] = lastUpdateElement.Substring(0, lastUpdateElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(string.Format("UPDATE {0}.{1}", instance.Schema, mainTable));
            queryElements.AppendLine("SET");
            queryElements.AppendLine(string.Join(Environment.NewLine, updateElements));
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine(string.Format("{0} = {1}", keyField, keyValue));

            return queryElements.ToString();
        }
    }
}
