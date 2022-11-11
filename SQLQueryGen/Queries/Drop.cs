using System;
using System.Reflection;

namespace SQLQueryGen.Query
{
    internal static partial class Generator
    {
        internal static string GenerateDropTableQuery<T>(IDatabase database)
        {
            if (database == null)
                return string.Empty;

            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            return database.GetDropQuery(Constants.DBObjectType.Table, mainTable);
        }
    }
}
