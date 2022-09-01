using System;

namespace SQLQueryGen
{
    public interface IDatabase
    {
        public string ConnectionString { get; }
        public string Schema { get; set; }

        public virtual string GetConnectionString()
        {
            return string.Empty;
        }

        public string GetCreateQuery(string table)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"CREATE TABLE IF NOT EXISTS {table}" :
                $"CREATE TABLE IF NOT EXISTS {Schema}.{table}";
        }

        public string GetInsertQuery(string table)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"INSERT INTO {table}" :
                $"INSERT INTO {Schema}.{table}";
        }

        public string GetUpdateQuery(string table)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"UPDATE {table}" :
                $"UPDATE {Schema}.{table}";
        }

        public string GetDeleteQuery(string table)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"DELETE FROM {table}" :
                $"DELETE FROM {Schema}.{table}";
        }

        public string GetSelectQueryFromElement(string table, string alias)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"FROM {table} {alias}" :
                $"FROM {Schema}.{table} {alias}";
        }

        public string GetSelectQueryJoinElement(string type, string table, string fieldJoin, string fieldMain, string aliasJoin, string aliasMain)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"{type} {table} {aliasJoin} ON {aliasJoin}.{fieldJoin} = {aliasMain}.{fieldMain}" :
                $"{type} {Schema}.{table} {aliasJoin} ON {aliasJoin}.{fieldJoin} = {aliasMain}.{fieldMain}";
        }

        public string GetKeyFieldType()
        {
            return string.Empty;
        }

        public string GetNavigateFieldType()
        {
            return string.Empty;
        }

        public virtual string GetFieldType(Type propertyType)
        {
            return string.Empty;
        }

        public virtual string GetFieldType(Type propertyType, int size)
        {
            return string.Empty;
        }

        public virtual string GetFieldValue(Type propertyType, object value)
        {
            return string.Empty;
        }
    }
}
