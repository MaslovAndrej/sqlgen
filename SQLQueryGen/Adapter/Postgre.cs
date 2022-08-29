using System;
using System.Collections.Generic;
using System.Text;

namespace SQLQueryGen.Adapter
{
    public class Postgre : Database
    {
        public override string GetConnectionString()
        {
            return base.GetConnectionString();
        }

        public override string GetFieldType(Type propertyType)
        {
            return base.GetFieldType();
        }

        public override string GetFieldType(Type propertyType, int size)
        {
            return base.GetFieldType();
        }
    }
}
