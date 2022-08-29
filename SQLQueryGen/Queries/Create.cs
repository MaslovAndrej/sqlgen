using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static partial class Generator
    {
        public static string GenerateCreateQuery<T>(DBInstance instance)
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
                    fieldElements.Add(string.Format("{0} serial4 NOT NULL,", fieldAttribute.Name));
                    continue;
                }

                if (navigateAttribute != null)
                {
                    if (navigateAttribute.Required)
                        fieldElements.Add(string.Format("\"{0}\" int4 NOT NULL,", fieldAttribute.Name));
                    else
                        fieldElements.Add(string.Format("\"{0}\" int4 NULL,", fieldAttribute.Name));
                    continue;
                }

                var fieldType = instance.GetFieldType(property.PropertyType, fieldAttribute.Size);

                fieldElements.Add(string.Format("\"{0}\" {1},", fieldAttribute.Name, fieldType));
            }

            var lastFieldElement = fieldElements.Last();
            fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine(string.Format($"CREATE TABLE IF NOT EXISTS {instance.Schema}.{mainTable}"));
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
            queryElements.AppendLine(")");

            return queryElements.ToString();
        }
    }
}
