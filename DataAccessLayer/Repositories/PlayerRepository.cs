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
        public PlayerModel GetPlayerById(int id)
        {
            string sql = "select * from player where id = @id";
            using var reader = ExecuteReader(sql, new MySqlParameter("@id", id));
            if (reader.Read())
            {
                string name = reader.MyGetString("name");

                PlayerModel model = new PlayerModel()
                {
                    Id = id,
                    Name = name,
                    MapId = reader.MyGetShort("mapid"),
                    X = reader.MyGetShort("x"),
                    Y = reader.MyGetShort("y"),
                    Stats = reader.MyGetString("stats"),
                    ClassType = (byte)reader.MyGetSbyte("classtype"),
                    Skills = reader.MyGetString("skills"),
                    Level = reader.MyGetShort("level"),
                    Exp = reader.MyGetLong("exp"),
                    Gender = (Gender)reader.MyGetSbyte("gender"),
                    InventoryItems = reader.MyGetString("inventory_items"),
                    WearingItems = reader.MyGetString("wearing_items"),
                    Gold = reader.MyGetLong("gold"),
                    Quest = reader.MyGetString("quest")
                };
                return model;
            }
            return null;
        }

        public int GetIdByUsernameAndPassword(string username, string password)
        {
            string sql = "select id from player where username = @username and password = @password";
            using var reader = ExecuteReader(sql,
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));
            if (reader.Read())
            {
                return reader.MyGetInt("id");
            }
            return -1;
        }
    }
}
