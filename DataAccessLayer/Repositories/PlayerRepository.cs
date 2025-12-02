using Crimson_Knight_Server.DataAccessLayer.Models;
using Crimson_Knight_Server.DataAccessLayer.Repositories.Bases;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Utils.Loggings;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.DataAccessLayer.Repositories
{
    public class PlayerRepository : BaseRepository
    {
        private static PlayerRepository ins;
        public static PlayerRepository GI()
        {
            if (ins == null)
            {
                ins = new PlayerRepository();
            }
            return ins;
        }


        public PlayerModel GetPlayerById(int id)
        {
            string sql = "select id, name from player where id = @id";
            using var reader = ExecuteReader(sql, new MySqlParameter("@id", id));
            if (reader.Read())
            {
                string name = reader.MyGetString("name");

                PlayerModel model = new PlayerModel()
                {
                    Id = id,
                    Name = name,
                };
            }
            return null;
        }
    }
}
