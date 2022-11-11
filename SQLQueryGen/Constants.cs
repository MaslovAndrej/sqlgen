using System;
using System.Collections.Generic;
using System.Text;

namespace SQLQueryGen
{
    public static class Constants
    {
        public static class Expression
        {
            public const string Equal = "=";
            public const string NotEqual = "<>";
            public const string Like = "like";
            public const string Less = "<";
            public const string LessOrEqual = "<=";
            public const string More = ">";
            public const string MoreOrEqual = ">=";
            public const string IsNull = "is null";
            public const string IsNotNull = "is not null";
        }

        public static class DBObjectType
        {
            public const string Schema = "SCHEMA";
            public const string Table = "TABLE";
        }
    }
}