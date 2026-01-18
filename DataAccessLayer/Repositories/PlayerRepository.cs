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
                    Quest = reader.MyGetString("quest"),
                    PotentialPoint = reader.MyGetInt("potentialpoint"),
                    SkillPoint = reader.MyGetInt("potentialpoint")
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

        public bool UpdatePlayer(PlayerModel model)
        {
            try
            {
                string sql = @"UPDATE player SET 
                    mapid = @mapid,
                    x = @x,
                    y = @y,
                    stats = @stats,
                    skills = @skills,
                    level = @level,
                    exp = @exp,
                    inventory_items = @inventory_items,
                    wearing_items = @wearing_items,
                    gold = @gold,
                    quest = @quest,
                    potentialpoint = @potentialpoint,
                    skillpoint = @skillpoint
                    WHERE id = @id";

                int rowsAffected = ExecuteNonQuery(sql,
                    new MySqlParameter("@id", model.Id),
                    new MySqlParameter("@mapid", model.MapId),
                    new MySqlParameter("@x", model.X),
                    new MySqlParameter("@y", model.Y),
                    new MySqlParameter("@stats", model.Stats),
                    new MySqlParameter("@skills", model.Skills),
                    new MySqlParameter("@level", model.Level),
                    new MySqlParameter("@exp", model.Exp),
                    new MySqlParameter("@inventory_items", model.InventoryItems),
                    new MySqlParameter("@wearing_items", model.WearingItems),
                    new MySqlParameter("@gold", model.Gold),
                    new MySqlParameter("@quest", model.Quest ?? (object)DBNull.Value),
                    new MySqlParameter("@potentialpoint", model.PotentialPoint),
                    new MySqlParameter("@skillpoint", model.SkillPoint)
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[UpdatePlayer] update player loi {model.Id}: {ex.Message}");
                return false;
            }
        }
    }
}
