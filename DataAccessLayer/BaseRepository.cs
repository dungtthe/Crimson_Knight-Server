using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.DataAccessLayer
{
    public class BaseRepository
    {
        protected MySqlConnection Connection;

        public BaseRepository()
        {
            Connection = new MySqlConnection(ServerSetting.ConnectionString);
            Connection.Open();
        }

        protected IDataReader ExecuteReader(string sql, params MySqlParameter[] parameters)
        {
            var cmd = new MySqlCommand(sql, Connection);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
