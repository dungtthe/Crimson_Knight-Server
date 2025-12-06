using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.DataAccessLayer.Repositories.Bases
{
    public static class DataReaderExtension
    {
        private static T GetValue<T>(this IDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
                return default!;

            return (T)Convert.ChangeType(reader.GetValue(index), typeof(T));
        }

        public static int MyGetInt(this IDataReader reader, string columnName)
        {
            return reader.GetValue<int>(columnName);   
        }
        public static short MyGetShort(this IDataReader reader, string columnName)
        {
            return reader.GetValue<short>(columnName);
        }
        public static short MyGetSbyte(this IDataReader reader, string columnName)
        {
            return reader.GetValue<sbyte>(columnName);
        }
        public static string MyGetString(this IDataReader reader, string columnName)
        {
            return reader.GetValue<string>(columnName);
        }
    }
}
