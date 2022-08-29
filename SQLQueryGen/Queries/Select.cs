using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static class QueryGenerator
    {

        #region GenerateSelect.

        public static string GenerateSelectQuery<T>(AddWhere<T> addWhere, AddOrder<T> addOrder, string direction)
        {
            var query = GenerateSelectQuery<T>(addWhere);

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(AddOrder<T> addOrder, string direction)
        {
            var query = GenerateSelectQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(AddWhere<T> addWhere)
        {
            var query = GenerateSelectQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateSelectQuery<T>();

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

        public static string GenerateSelectQuery<T>()
        {
            var selectElementTemplate = "{0}.{1} as {2},";
            var fromElementTemplate = "FROM {0}.{1} {2}";
            var joinElementTemplate = @"{0} {1}.{2} {3} ON {3}.{4} = {5}.{6}";

            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();
            var fromElement = string.Format(fromElementTemplate, DBInitializer.Schema, mainTable, mainAlias);

            var selectElements = GetSelectElements(type, mainAlias);
            var joinElements = new List<string>();
            var joinAliases = new List<string>();

            var properties = type.GetProperties()
              .Where(x => x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                var selectElement = string.Format(selectElementTemplate, mainAlias, fieldAttribute.Name, property.Name);
                selectElements.Add(selectElement);

                if (navigateAttribute != null)
                {
                    var joinType = "JOIN";
                    if (!navigateAttribute.Required)
                        joinType = "LEFT JOIN";

                    var joinPostfix = 0;
                    var joinAlias = navigateAttribute.TableName.Substring(0, 3).ToLower();
                    while (joinAliases.Contains(joinAlias))
                    {
                        joinPostfix++;
                        joinAlias = joinAlias + joinPostfix.ToString();
                    }
                    joinAliases.Add(joinAlias);

                    var joinElement = string.Format(joinElementTemplate, joinType, DBInitializer.Schema, navigateAttribute.TableName,
                                                                         joinAlias, navigateAttribute.FieldName, mainAlias, fieldAttribute.Name);
                    joinElements.Add(joinElement);

                    var propertyType = property.PropertyType;
                    selectElements.AddRange(GetSelectElements(propertyType, joinAlias));
                }
            }

            var lastElement = selectElements.Last();
            selectElements[selectElements.FindIndex(x => x.Equals(lastElement))] = lastElement.Substring(0, lastElement.Length - 1);

            var queryElements = new List<string>();
            queryElements.Add("SELECT");
            queryElements.Add(string.Join(Environment.NewLine, selectElements));
            queryElements.Add(fromElement);
            if (joinElements.Any())
                queryElements.Add(string.Join(Environment.NewLine, joinElements));

            return string.Join(Environment.NewLine, queryElements);
        }

        private static List<string> GetSelectElements(Type type, string alias)
        {
            var selectElementTemplate = "{0}.{1} as {2},";

            var selectElements = new List<string>();

            var onlyFieldProperies = type.GetProperties()
              .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute) &&
                          !x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));

            foreach (var property in onlyFieldProperies)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

                var selectElement = string.Format(selectElementTemplate, alias, fieldAttribute.Name, property.Name);
                selectElements.Add(selectElement);
            }

            return selectElements;
        }

        #endregion

    }

}
