using System;
using SQLQueryGen;
using SQLQueryGen.Adapter;

namespace SQLGenTest
{
    public static class Config
    {
        public static SQLite Database { get; set; }
        public static Generator QueryGenerator { get; set; }

        public static void InitDB(string fileName)
        {
            Database = new SQLite(fileName, null);
            QueryGenerator = new Generator(Database);
        }
    }
}
