using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Utils.Loggings;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.DataAccessLayer
{
    public class PlayerRepository:BaseRepository
    {
        public Player GetPlayerById(int id)
        {
            string sql = "select id, name from player where id = @id";
            using var reader = ExecuteReader(sql, new MySqlParameter("@id", id));
            if (reader.Read())
            {
                string name = reader.MyGetString("name");
                ConsoleLogging.LogInfor(name);
            }
            return null; 
        }
    }
}
