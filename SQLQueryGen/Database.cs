using System;

namespace SQLQueryGen
{
    public interface IDatabase
    {
        string ConnectionString { get; }
        string Schema { get; set; }

        string GetCreateQuery(string table);
        string GetInsertQuery(string table);
        string GetUpdateQuery(string table);
        string GetDeleteQuery(string table);
        string GetSelectQueryFromElement(string table, string alias);
        string GetSelectQueryJoinElement(string type, string table, string fieldJoin, string fieldMain, string aliasJoin, string aliasMain);
        string GetDropQuery(string type, string name);
        string GetKeyFieldType();
        string GetNavigateFieldType();
        string GetFieldType(Type propertyType);
        string GetFieldType(Type propertyType, int size);
        string GetFieldValue(Type propertyType, object value);
    }
}
