using System;
using System.Collections.Generic;
using System.Text;

namespace SQLQueryGen.Adapter
{
    public class PostgreSQL : IDatabase
    {
        public string Schema { get; set; }
        public string ConnectionString { get; }

        private string GetConnectionString(string server, string port, string user, string password, string database)
        {
            return string.Format("Host={0};Port={1};Username={2};Password={3};Database={4}", server, port, user, password, database);
        }

        public string GetKeyFieldType()
        {
            return "serial4";
        }

        public string GetNavigateFieldType()
        {
            return "int4";
        }

        public string GetFieldType(Type propertyType)
        {
            return GetFieldType(propertyType, 0);
        }

        public string GetFieldType(Type propertyType, int size)
        {
            var fieldType = string.Empty;

            if (propertyType == typeof(string))
                fieldType = "varchar({0}) NULL";
            if (propertyType == typeof(short))
                fieldType = "smallint NOT NULL";
            if (propertyType == typeof(short?))
                fieldType = "smallint NULL";
            if (propertyType == typeof(int))
                fieldType = "integer NOT NULL";
            if (propertyType == typeof(int?))
                fieldType = "integer NULL";
            if (propertyType == typeof(long))
                fieldType = "bigint NOT NULL";
            if (propertyType == typeof(long?))
                fieldType = "bigint NULL";
            if (propertyType == typeof(float))
                fieldType = "real NOT NULL";
            if (propertyType == typeof(float?))
                fieldType = "real NULL";
            if (propertyType == typeof(double))
                fieldType = "double precision NOT NULL";
            if (propertyType == typeof(double?))
                fieldType = "double precision NULL";
            if (propertyType == typeof(decimal))
                fieldType = "money NOT NULL";
            if (propertyType == typeof(decimal?))
                fieldType = "money NULL";
            if (propertyType == typeof(bool))
                fieldType = "bool NOT NULL";
            if (propertyType == typeof(bool?))
                fieldType = "bool NULL";
            if (propertyType == typeof(DateTime))
                fieldType = "timestamp NOT NULL";
            if (propertyType == typeof(DateTime?))
                fieldType = "timestamp NULL";

            if (fieldType.Contains("{0}"))
            {
                if (size > 0)
                    fieldType = string.Format(fieldType, size);
                else
                    fieldType = fieldType.Replace("({0})", string.Empty);
            }

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

        public PostgreSQL(string server, int port, string user, string password, string database, string schema)
        {
            this.Schema = schema;
            this.ConnectionString = GetConnectionString(server, port.ToString(), user, password, database);
        }
    }
}
