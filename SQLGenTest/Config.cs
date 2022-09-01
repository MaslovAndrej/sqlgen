using System;
using SQLQueryGen;
using SQLQueryGen.Adapter;

namespace SQLGenTest
{
    public static class Config
    {
        public static Sqlite Database { get; set; }
        public static Generator QueryGenerator { get; set; }

        public static void InitDB(string fileName)
        {
            Database = new Sqlite(fileName, null);
            QueryGenerator = new Generator(Database);
        }
    }
}
