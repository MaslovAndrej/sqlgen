using System;
using SQLQueryGen;
using System.Data.SQLite;
using Dapper;

namespace SQLGenTest
{
    public static class Cities
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

        public static void Save(this City city)
        {
            if (city.Id > 0)
                Update(city);
            else
                Insert(city);
        }

        private static void Insert(City city)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateInsertQuery<City>(city);

                if (debugMode)
                    Console.WriteLine(query);

                city.Id = connection.QuerySingle<int>(query);
            }
        }

        private static void Update(City city)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateUpdateQuery<City>(city);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion

        #region Get

        public static List<City> Get()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateSelectQuery<City>();

                if (debugMode)
                    Console.WriteLine(query);

                return connection.Query<City, Region, Country, City>(
                query,
                (city, linkedRegion, linkedCountry) =>
                {
                    city.Region = linkedRegion != null && linkedRegion.Id > 0 ? linkedRegion : null;
                    city.Country = linkedCountry != null && linkedCountry.Id > 0 ? linkedCountry : null;
                    return city;
                },
                splitOn: "Region, Country").ToList();
            }
        }

        public static City Get(int id)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateSelectQuery<City>(new AddWhere<City>("Id", Constants.Expression.Equal, id));

                if (debugMode)
                    Console.WriteLine(query);

                return connection.Query<City, Region, Country, City>(
                query,
                (city, linkedRegion, linkedCountry) =>
                {
                    city.Region = linkedRegion != null && linkedRegion.Id > 0 ? linkedRegion : null;
                    city.Country = linkedCountry != null && linkedCountry.Id > 0 ? linkedCountry : null;
                    return city;
                },
                splitOn: "Region, Country").FirstOrDefault();
            }
        }

        public static City Get(string name)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateSelectQuery<City>(new AddWhere<City>("Name", Constants.Expression.Equal, name));

                if (debugMode)
                    Console.WriteLine(query);

                return connection.Query<City, Region, Country, City>(
                query,
                (city, linkedRegion, linkedCountry) =>
                {
                    city.Region = linkedRegion != null && linkedRegion.Id > 0 ? linkedRegion : null;
                    city.Country = linkedCountry != null && linkedCountry.Id > 0 ? linkedCountry : null;
                    return city;
                },
                splitOn: "Region, Country").FirstOrDefault();
            }
        }

        #endregion

        #region Delete

        public static void Delete(City city)
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateDeleteQuery<City>(city);

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
                city = null;
            }
        }

        #endregion

        #region Create

        public static void Create()
        {
            using (var connection = new SQLiteConnection(Config.Database.ConnectionString))
            {
                var query = Config.QueryGenerator.GenerateCreateQuery<City>();

                if (debugMode)
                    Console.WriteLine(query);

                connection.Execute(query);
            }
        }

        #endregion
    }
}
