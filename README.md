# sqlgen
The simplest library for generating SQL queries.
Supports: SQLite, PostgreSQL.

Example:
```
// Object
[Table("city")]
public class City
{
    [Field("id", Key = true)]
    public int Id { get; set; }

    [Field("code", Size = 10)]
    public string Code { get; set; }

    [Field("name", Size = 100)]
    public string Name { get; set; }

    [Field("region")]
    [Navigate(TableName = "region", FieldName = "id", Required = false)]
    public Region Region { get; set; }

    [Field("country")]
    [Navigate(TableName = "country", FieldName = "id", Required = true)]
    public Country Country { get; set; }
}

// Config
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

// Create query
var query = Config.QueryGenerator.GenerateCreateQuery<City>();

// Select query
var query = Config.QueryGenerator.GenerateSelectQuery<City>();

// Insert query
var query = Config.QueryGenerator.GenerateInsertQuery<City>(city);

// Update query
var query = Config.QueryGenerator.GenerateUpdateQuery<City>(city);

// Delete query
var query = Config.QueryGenerator.GenerateDeleteQuery<City>(city);
```
