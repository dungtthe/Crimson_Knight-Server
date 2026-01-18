using Crimson_Knight_Server.DataAccessLayer.Models;
using Crimson_Knight_Server.DataAccessLayer.Repositories;
using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Services.Dtos;
using Crimson_Knight_Server.Services.Mappers;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Services
{
    public static class PlayerService
    {
        private static List<string> LoginTokens = new List<string>();

        private static readonly object _lock = new object();
        public static LoginResponse Login(LoginRequest request)
        {
            if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse
                {
                    HttpStatusCode = 400,
                    Message = "Thông tin tài khoản hoặc mật khẩu không chính xác"
                };
            }

            request.Username = request.Username.ToLower().Trim();
            request.Password = request.Password.ToLower().Trim();

            lock (_lock)
            {
                //check het han
                List<string> rsRemove = new List<string>();
                foreach (var token in LoginTokens)
                {
                    string[] s = token.Split('.');
                    long timeMillis = long.Parse(s[1]);
                    if (SystemUtil.CurrentTimeMillis() - timeMillis > 10000)
                    {
                        rsRemove.Add(token);
                    }
                }
                foreach (var token in rsRemove)
                {
                    LoginTokens.Remove(token);
                }

                //check dang nhap
                PlayerRepository playerRepository = new PlayerRepository();

                var playerId = playerRepository.GetIdByUsernameAndPassword(request.Username, request.Password);
                if (playerId == -1)
                {
                    return new LoginResponse
                    {
                        HttpStatusCode = 404,
                        Message = "Thông tin tài khoản hoặc mật khẩu không chính xác"
                    };
                }

                if (ServerManager.GI().GetPlayerById(playerId) != null)
                {
                    return new LoginResponse
                    {
                        HttpStatusCode = 400,
                        Message = "Bạn đã đăng nhập ở nơi khác rồi!"
                    };
                }

                foreach(var saveDataMsg in ServerManager.GI().SaveDataMsgs)
                {
                    if(saveDataMsg.Item1.Id == playerId)
                    {
                        return new LoginResponse
                        {
                            HttpStatusCode = 400,
                            Message = "Đợi chút nữa rùi vào nhé!"
                        };
                    }
                }

                foreach (var token in LoginTokens)
                {
                    string[] s = token.Split('.');
                    if (s[0] == playerId.ToString())
                    {
                        return new LoginResponse
                        {
                            HttpStatusCode = 400,
                            Message = "Vui lòng đợi một lát!"
                        };
                    }
                }

                string tokenGenerate = GenerateToken(playerId);
                LoginTokens.Add(tokenGenerate);

                return new LoginResponse
                {
                    HttpStatusCode = 200,
                    Message = tokenGenerate
                };
            }
        }


        static string GenerateToken(int playerId)
        {
            //tam thoi nhu nay
            return playerId.ToString() + "." + SystemUtil.CurrentTimeMillis().ToString();
        }


        public static string ValidateToken(string token)
        {
            //tam thoi nhu nay
            return token;
        }


        public static bool SetupPlayer(Player player, int playerId)
        {
            PlayerRepository playerRepository = new PlayerRepository();
            var playerModel = playerRepository.GetPlayerById(playerId);
            if (playerModel == null) return false;
            player.SetId(playerModel.Id);
            player.Name = playerModel.Name;
            player.ClassType = (ClassType)playerModel.ClassType;
            player.Level = playerModel.Level;
            player.Exp = playerModel.Exp;
            player.Gender = playerModel.Gender;
            player.Gold = playerModel.Gold;
            player.PotentialPoint = playerModel.PotentialPoint;
            player.SkillPoint = playerModel.SkillPoint;
            player.SetUpStats(playerModel.Stats);
            player.SetUpSkills(playerModel.Skills);
            player.SetUpItem(playerModel.InventoryItems, playerModel.WearingItems);
            player.SetUpQuest(playerModel.Quest);
            ServerManager.GI().AddSession(player);
            PlayerEnterGame(player, playerModel);
            return true;
        }

        private static void PlayerEnterGame(Player player, PlayerModel model)
        {
            player.X = model.X;
            player.Y = model.Y;
            Map map = MapManager.Maps[model.MapId];
            //map.BusPlayerEnterMap.Enqueue(player);
            MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool,short, short>(map, player, true, player.X, player.Y));
        }

        public static void SaveData(Player player, short mapId, short x, short y)
        {
            try
            {
                object[] inven = new object[player.InventoryItems.Length];

                for (int i = 0; i < inven.Length; i++)
                {
                    BaseItem item = player.InventoryItems[i];
                    if (item == null) continue;

                    inven[i] = new object[]
                    {
                        (byte)item.GetItemType(),
                        item switch
                        {
                            ItemEquipment e  => e.ToSaveData(),
                            ItemConsumable c => c.ToSaveData(),
                            ItemMaterial m   => m.ToSaveData(),
                            _ => null
                        }
                    };
                }

                string inventoryJson = JsonSerializer.Serialize(inven);

                object[] wear = new object[player.WearingItems.Length];

                for (int i = 0; i < wear.Length; i++)
                {
                    ItemEquipment item = player.WearingItems[i];
                    if (item == null) continue;
                    wear[i] = item.ToSaveData();
                }

                string wearingJson = JsonSerializer.Serialize(wear);

                string skillsString = string.Join("_", player.Skills.Select(s => $"{s.TemplateId}.{s.VariantId}"));

                string statsJson = Helpers.SerializeStats(player.Stats);

                string questString = null;
                if (player.Quest != null)
                {
                    questString = $"{player.Quest.Id}.{player.Quest.QuantityCur}.{(byte)player.Quest.QuestState}";
                }

                PlayerModel playerModel = new PlayerModel()
                {
                    Id = player.Id,
                    MapId = mapId,
                    X = x,
                    Y = y,
                    Stats = statsJson,
                    Skills = skillsString,
                    Level = player.Level,
                    Exp = player.Exp,
                    InventoryItems = inventoryJson,
                    WearingItems = wearingJson,
                    Gold = player.Gold,
                    Quest = questString,
                    PotentialPoint = player.PotentialPoint,
                    SkillPoint = player.SkillPoint
                };

                PlayerRepository repository = new PlayerRepository();
                repository.UpdatePlayer(playerModel);
                repository.Dispose();

                ConsoleLogging.LogInfor($"[SaveData] Player {player.Name} (ID: {player.Id}) thanh cong");
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[SaveData] loi save player {player.Id}: {ex.Message}");
            }
        }

    }
}
