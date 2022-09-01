using System;
using SQLQueryGen;

namespace SQLGenTest
{
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
}
