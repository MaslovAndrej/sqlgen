using System;
using System.Data.SQLite;
using Dapper;

namespace SQLGenTest
{
    public static class Countries
    {
        #region DebugMode

        private static bool debugMode = false;

        public static void SetDebugModeOn()
        {
            debugMode = true;
        }

        public static void SetDebugModeOff()
        {
            debugMode = false;
        }

        #endregion

        #region Save

        public static void Save(this Country country)
        {
            if (country.Id > 0)
                Update(country);
            else
                Insert(country);
        }

        private static void Insert(Country country)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateInsertQuery<Country>(country);

                if (debugMode)
                    Console.WriteLine(query);

                country.Id = connection.QuerySingle<int>(query);
            }
        }

        private static void Update(Country country)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateUpdateQuery<Country>(country);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion

        #region Get

        public static List<Country> Get()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateSelectQuery<Country>();

                if (debugMode)
                    Console.WriteLine(query);

                return connection.Query<Country>(query).ToList();
            }
        }

        #endregion

        #region Delete

        public static void Delete()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateDeleteQuery<Country>();

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        public static void Delete(Country country)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateDeleteQuery<Country>(country);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
                country = null;
            }
        }

        #endregion

        #region Create

        public static void Create()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateCreateQuery<Country>();

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion

        #region DropTable

        public static void Drop()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateDropTableQuery<City>();

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion
    }
}
