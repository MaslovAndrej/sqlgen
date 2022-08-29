using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static partial class Generator
    {
        public static string GenerateInsertQuery<T>(DBInstance instance, T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var fieldElements = new List<string>();
            var valueElements = new List<string>();

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

                fieldElements.Add(string.Format("\"{0}\",", fieldAttribute.Name));
                valueElements.Add(instance.GetFieldValue(property.PropertyType, value) + ",");
            }

            var lastFieldElement = fieldElements.Last();
            fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

            var lastValueElement = valueElements.Last();
            valueElements[valueElements.Count - 1] = lastValueElement.Substring(0, lastValueElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(string.Format("INSERT INTO {0}.{1}", instance.Schema, mainTable));
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
            queryElements.AppendLine(")");
            queryElements.AppendLine("VALUES");
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, valueElements));
            queryElements.AppendLine(")");

            if (!string.IsNullOrEmpty(keyField))
                queryElements.AppendLine(string.Format("RETURNING {0}", keyField));

            return queryElements.ToString();
        }
    }
}
