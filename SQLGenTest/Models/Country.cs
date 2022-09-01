using System;
using SQLQueryGen;

namespace SQLGenTest
{
    [Table("country")]
    public class Country
    {
        [Field("id", Key = true)]
        public int Id { get; set; }

        [Field("name", Size = 100)]
        public string Name { get; set; }
    }
}
