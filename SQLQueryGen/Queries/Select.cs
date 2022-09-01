using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen.Query
{
    internal static partial class Generator
    {
        internal static string GenerateSelectQuery<T>(IDatabase database)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();
            var fromElement = database.GetSelectQueryFromElement(mainTable, mainAlias);

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

                var selectElement = $"{mainAlias}.{fieldAttribute.Name} as {property.Name},";
                selectElements.Add(selectElement);

                if (navigateAttribute != null)
                {
                    var joinType = "LEFT JOIN";
                    if (navigateAttribute.Required)
                        joinType = "JOIN";

                    var joinPostfix = 0;
                    var joinAlias = navigateAttribute.TableName.Substring(0, 3).ToLower();
                    while (joinAliases.Contains(joinAlias))
                    {
                        joinPostfix++;
                        joinAlias = joinAlias + joinPostfix.ToString();
                    }
                    joinAliases.Add(joinAlias);

                    var joinElement = database.GetSelectQueryJoinElement(joinType, navigateAttribute.TableName,
                                                                         navigateAttribute.FieldName, fieldAttribute.Name, joinAlias, mainAlias);
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

        internal static string GenerateSelectQuery<T>(IDatabase database, AddOrder<T> addOrder)
        {
            var query = GenerateSelectQuery<T>(database);

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(addOrder.Direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        internal static string GenerateSelectQuery<T>(IDatabase database, AddWhere<T> addWhere)
        {
            addWhere.Database = database;

            var query = GenerateSelectQuery<T>(database);

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        internal static string GenerateSelectQuery<T>(IDatabase database, AddWhere<T> addWhere, AddOrder<T> addOrder)
        {
            var query = GenerateSelectQuery<T>(database, addWhere);

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(addOrder.Direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        internal static string GenerateSelectQuery<T>(IDatabase database, List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateSelectQuery<T>(database);

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

        private static List<string> GetSelectElements(Type type, string alias)
        {
            var selectElements = new List<string>();

            var onlyFieldProperies = type.GetProperties()
              .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute) &&
                          !x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));

            foreach (var property in onlyFieldProperies)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

                var selectElement = $"{alias}.{fieldAttribute.Name} as {property.Name},";
                selectElements.Add(selectElement);
            }

            return selectElements;
        }
    }
}
