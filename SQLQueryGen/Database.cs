using System;

namespace SQLQueryGen
{
    public abstract class DBInstance
    {
        public string ConnectionString { get; }
        public string Schema { get; set; }

        public string CreateQueryTemplate { get; set; }
        public string InsertQueryTemplate { get; set; }
        public string UpdateQueryTemplate { get; set; }

        public virtual string GetConnectionString()
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

        public virtual string GetCreateQueryTemplate()
        {
            return string.Empty;
        }

        public virtual string GetInsertQueryTemplate()
        {
            return string.Empty;
        }

        public virtual string GetUpdateQueryTemplate()
        {
            return string.Empty;
        }

        public DBInstance()
        {
            this.ConnectionString = GetConnectionString();
            this.CreateQueryTemplate = GetCreateQueryTemplate();
            this.InsertQueryTemplate = GetInsertQueryTemplate();
            this.UpdateQueryTemplate = GetUpdateQueryTemplate();
        }
    }
}
