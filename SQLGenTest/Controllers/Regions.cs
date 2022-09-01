using System;
using System.Data.SQLite;
using Dapper;

namespace SQLGenTest
{
    public static class Regions
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

        public static void Save(this Region region)
        {
            if (region.Id > 0)
                Update(region);
            else
                Insert(region);
        }

        private static void Insert(Region region)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateInsertQuery<Region>(region);

                if (debugMode)
                    Console.WriteLine(query);

                region.Id = connection.QuerySingle<int>(query);
            }
        }

        private static void Update(Region region)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateUpdateQuery<Region>(region);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion

        #region Get

        public static List<Region> Get()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateSelectQuery<Region>();

                if (debugMode)
                    Console.WriteLine(query);

                return connection.Query<Region, Country, Region>(
                query,
                (region, linkedCountry) =>
                {
                    region.Country = linkedCountry != null && linkedCountry.Id > 0 ? linkedCountry : null;
                    return region;
                },
                splitOn: "Country").ToList();
            }
        }

        #endregion

        #region Delete

        public static void Delete(Region region)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateDeleteQuery<Region>(region);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
                region = null;
            }
        }

        #endregion

        #region Create

        public static void Create()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateCreateQuery<Region>();

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion
    }
}
