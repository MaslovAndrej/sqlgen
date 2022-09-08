using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public class Generator
    {
        private IDatabase Database { get; set; }
        private static Generator instance;

        public string GenerateCreateQuery<T>()
        {
            return SQLQueryGen.Query.Generator.GenerateCreateQuery<T>(this.Database);
        }

        public string GenerateInsertQuery<T>(T entity)
        {
            return SQLQueryGen.Query.Generator.GenerateInsertQuery<T>(this.Database, entity);
        }

        public string GenerateUpdateQuery<T>(T entity)
        {
            return SQLQueryGen.Query.Generator.GenerateUpdateQuery<T>(this.Database, entity);
        }

        public string GenerateDeleteQuery<T>()
        {
            return SQLQueryGen.Query.Generator.GenerateDeleteQuery<T>(this.Database);
        }

        public string GenerateDeleteQuery<T>(AddWhere<T> addWhere)
        {
            return SQLQueryGen.Query.Generator.GenerateDeleteQuery<T>(this.Database, new AddWhere<T>(this.Database, addWhere));
        }

        public string GenerateDeleteQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var newAddWhereList = new List<AddWhere<T>>();
            foreach (var addWhere in addWhereList)
                newAddWhereList.Add(new AddWhere<T>(this.Database, addWhere));

            return SQLQueryGen.Query.Generator.GenerateDeleteQuery<T>(this.Database, newAddWhereList, addWhereCondition);
        }

        public string GenerateDeleteQuery<T>(T entity)
        {
            return SQLQueryGen.Query.Generator.GenerateDeleteQuery<T>(this.Database, entity);
        }

        public string GenerateSelectQuery<T>()
        {
            return SQLQueryGen.Query.Generator.GenerateSelectQuery<T>(this.Database);
        }

        public string GenerateSelectQuery<T>(AddOrder<T> addOrder)
        {
            return SQLQueryGen.Query.Generator.GenerateSelectQuery<T>(this.Database, addOrder);
        }

        public string GenerateSelectQuery<T>(AddWhere<T> addWhere)
        {
            return SQLQueryGen.Query.Generator.GenerateSelectQuery<T>(this.Database, new AddWhere<T>(this.Database, addWhere));
        }

        public string GenerateSelectQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var newAddWhereList = new List<AddWhere<T>>();
            foreach (var addWhere in addWhereList)
                newAddWhereList.Add(new AddWhere<T>(this.Database, addWhere));

            return SQLQueryGen.Query.Generator.GenerateSelectQuery<T>(this.Database, newAddWhereList, addWhereCondition);
        }

        public string GenerateSelectQuery<T>(AddWhere<T> addWhere, AddOrder<T> addOrder)
        {
            return SQLQueryGen.Query.Generator.GenerateSelectQuery<T>(this.Database, new AddWhere<T>(this.Database, addWhere), addOrder);
        }

        public string GenerateDropTableQuery<T>()
        {
            return SQLQueryGen.Query.Generator.GenerateDropTableQuery<T>(this.Database);
        }

        public static Generator GetInstance(IDatabase database)
        {
            if (instance == null)
                instance = new Generator(database);

            return instance;
        }

        public Generator(IDatabase database)
        {
            Database = database;
        }
    }
}
