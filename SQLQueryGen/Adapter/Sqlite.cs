﻿using System;
using System.IO;

namespace SQLQueryGen.Adapter
{
    public class SQLite : IDatabase
    {
        public string Schema { get; set; }
        public string FilePath { get; }
        public string ConnectionString { get; }

        private string GetConnectionString(string filePath)
        {
            return $"Data Source={filePath}";
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

        public string GetDropQuery(string type, string name)
        {
            return string.IsNullOrEmpty(Schema) ?
                $"DROP {type} IF EXISTS {name}" :
                $"DROP {type} IF EXISTS {Schema}.{name}";
        }

        public string GetKeyFieldType()
        {
            return "INTEGER PRIMARY KEY";
        }

        public string GetNavigateFieldType()
        {
            return "INTEGER";
        }

        public string GetFieldType(Type propertyType)
        {
            return GetFieldType(propertyType, 0);
        }

        public string GetFieldType(Type propertyType, int size)
        {
            var fieldType = string.Empty;

            if (propertyType == typeof(string))
                fieldType = "TEXT NULL";
            if (propertyType == typeof(short))
                fieldType = "INTEGER NOT NULL";
            if (propertyType == typeof(short?))
                fieldType = "INTEGER NULL";
            if (propertyType == typeof(int))
                fieldType = "INTEGER NOT NULL";
            if (propertyType == typeof(int?))
                fieldType = "INTEGER NULL";
            if (propertyType == typeof(long))
                fieldType = "INTEGER NOT NULL";
            if (propertyType == typeof(long?))
                fieldType = "INTEGER NULL";
            if (propertyType == typeof(double))
                fieldType = "REAL NOT NULL";
            if (propertyType == typeof(double?))
                fieldType = "REAL NULL";
            if (propertyType == typeof(decimal))
                fieldType = "TEXT NOT NULL";
            if (propertyType == typeof(decimal?))
                fieldType = "TEXT NULL";
            if (propertyType == typeof(bool))
                fieldType = "INTEGER NOT NULL";
            if (propertyType == typeof(bool?))
                fieldType = "INTEGER NULL";
            if (propertyType == typeof(DateTime))
                fieldType = "TEXT NOT NULL";
            if (propertyType == typeof(DateTime?))
                fieldType = "TEXT NULL";

            return fieldType;
        }

        public string GetFieldValue(Type propertyType, object value)
        {
            string fieldValue;

            if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(double))
                fieldValue = value.ToString().Replace(",", ".");
            else if (propertyType == typeof(DateTime))
                fieldValue = string.Format("'{0}'", (value as DateTime?).Value.ToString("yyyy-MM-dd HH:mm:ss"));
            else
                fieldValue = string.Format("'{0}'", value);

            return fieldValue;
        }

        public SQLite(string fileName, string schema)
        {
            this.Schema = schema;
            this.FilePath = Path.Combine(AppContext.BaseDirectory, fileName);
            this.ConnectionString = GetConnectionString(this.FilePath);
        }

        public SQLite(string path, string fileName, string schema)
        {
            this.Schema = schema;
            this.FilePath = Path.Combine(path, fileName);
            this.ConnectionString = GetConnectionString(this.FilePath);
        }
    }
}
