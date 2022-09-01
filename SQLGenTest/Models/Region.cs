using System;
using SQLQueryGen;

namespace SQLGenTest
{
    [Table("region")]
    public class Region
    {
        [Field("id", Key = true)]
        public int Id { get; set; }

        [Field("name", Size = 100)]
        public string Name { get; set; }

        [Field("number")]
        public int Number { get; set; }

        [Field("country")]
        [Navigate(TableName = "country", FieldName = "id", Required = true)]
        public Country Country { get; set; }
    }
}
