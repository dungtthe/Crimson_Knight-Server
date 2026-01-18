using Crimson_Knight_Server.DataAccessLayer.Models;
using Crimson_Knight_Server.DataAccessLayer.Repositories;
using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Services.Dtos;
using Crimson_Knight_Server.Services.Mappers;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Templates.Item;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        static string version = "e8eb7ee5-3abc-4da5-8869-15cd10cf3b23";
        public static LoginResponse Login(LoginRequest request)
        {
            if(version != request.Version)
            {
                return new LoginResponse
                {
                    HttpStatusCode = 400,
                    Message = "Vui lòng cập nhật phiên bản mới nhất"
                };
            }
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

                foreach (var saveDataMsg in ServerManager.GI().SaveDataMsgs)
                {
                    if (saveDataMsg.Item1.Id == playerId)
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

                playerRepository.Dispose();
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
            MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool, short, short>(map, player, true, player.X, player.Y));
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


        private static object objLockRegister = new object();
        public static RegisterRespone Register(RegisterRequest data)
        {
            try
            {
                if (data.UserName != null)
                {
                    data.UserName.Trim();
                }
                if (data.Password != null)
                {
                    data.Password.Trim();
                }
                if (string.IsNullOrEmpty(data.UserName) || string.IsNullOrEmpty(data.Password))
                {
                    return new RegisterRespone
                    {
                        HttpStatusCode = 400,
                        Message = "Tên tài khoản hoặc mật khẩu không được để trống"
                    };
                }

                if (data.ClassType == ClassType.NONE)
                {
                    return new RegisterRespone
                    {
                        HttpStatusCode = 400,
                        Message = "Class không hợp lệ"
                    };
                }

                if (data.Gender == Gender.Unisex)
                {
                    return new RegisterRespone
                    {
                        HttpStatusCode = 400,
                        Message = "Giới tính không hợp lệ"
                    };
                }

                lock (objLockRegister)
                {
                    PlayerRepository playerRepository = new PlayerRepository();

                    if (playerRepository.IsUsernameExists(data.UserName))
                    {
                        return new RegisterRespone
                        {
                            HttpStatusCode = 400,
                            Message = "Tên tài khoản đã tồn tại"
                        };
                    }

                    PlayerModel model = new PlayerModel();
                    model.UserName = data.UserName;
                    model.Password = data.Password;
                    model.Name = data.UserName;
                    model.MapId = 0;
                    model.X = 744;
                    model.Y = 486;

                    Dictionary<StatId, Stat> Stats = new Dictionary<StatId, Stat>();
                    Stats.Add(StatId.HP, new Stat() { Id = StatId.HP, Value = 300 });
                    Stats.Add(StatId.MP, new Stat() { Id = StatId.MP, Value = 300 });
                    Stats.Add(StatId.ATK, new Stat() { Id = StatId.ATK, Value = 30 });
                    Stats.Add(StatId.DEF, new Stat() { Id = StatId.DEF, Value = 5 });

                    model.Stats = Helpers.SerializeStats(Stats);
                    model.ClassType = (byte)data.ClassType;
                    model.Skills = "0.0_1.-1_2.-1";
                    model.Gender = data.Gender;

                    ItemEquipment[] WearingItems = new ItemEquipment[4];
                    ItemEquipmentTemplate equipmentTemplate = null;
                    ItemEquipmentTemplate wingTemplate = null;

                    if (data.ClassType == ClassType.CHIEN_BINH)
                    {
                        equipmentTemplate = TemplateManager.ItemEquipmentTemplates[0];
                        wingTemplate = TemplateManager.ItemEquipmentTemplates[24];
                    }
                    else if (data.ClassType == ClassType.SAT_THU)
                    {
                        equipmentTemplate = TemplateManager.ItemEquipmentTemplates[1];
                        wingTemplate = TemplateManager.ItemEquipmentTemplates[25];
                    }
                    else if (data.ClassType == ClassType.PHAP_SU)
                    {
                        equipmentTemplate = TemplateManager.ItemEquipmentTemplates[2];
                        wingTemplate = TemplateManager.ItemEquipmentTemplates[26];
                    }
                    else
                    {
                        equipmentTemplate = TemplateManager.ItemEquipmentTemplates[3];
                        wingTemplate = TemplateManager.ItemEquipmentTemplates[27];
                    }
                    WearingItems[0] = new ItemEquipment(Helpers.GenerateId(), equipmentTemplate.Id);
                    if (data.Gender == Gender.Male)
                    {
                        WearingItems[1] = new ItemEquipment(Helpers.GenerateId(), 4);
                        WearingItems[2] = new ItemEquipment(Helpers.GenerateId(), 6);
                    }
                    else
                    {
                        WearingItems[1] = new ItemEquipment(Helpers.GenerateId(), 5);
                        WearingItems[2] = new ItemEquipment(Helpers.GenerateId(), 7);
                    }

                    object[] wear = new object[WearingItems.Length];
                    for (int i = 0; i < wear.Length; i++)
                    {
                        ItemEquipment item = WearingItems[i];
                        if (item == null) continue;
                        wear[i] = item.ToSaveData();
                    }
                    string wearingJson = JsonSerializer.Serialize(wear);
                    model.WearingItems = wearingJson;

                    model.Level = 1;
                    model.Exp = 0;
                    model.Gold = 1000000;



                    ItemEquipment wing = null;
                    wing = new ItemEquipment(Helpers.GenerateId(), wingTemplate.Id);
                    BaseItem[] InventoryItems = new BaseItem[48];
                    InventoryItems[0] = wing;

                    object[] inven = new object[InventoryItems.Length];

                    for (int i = 0; i < inven.Length; i++)
                    {
                        BaseItem item = InventoryItems[i];
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
                    model.InventoryItems = inventoryJson;
                    model.PotentialPoint = 10;
                    model.SkillPoint = 10;
                    model.Quest = null;

                    int newPlayerId = playerRepository.InsertPlayer(model);
                    playerRepository.Dispose();

                    if (newPlayerId == -1)
                    {
                        return new RegisterRespone
                        {
                            HttpStatusCode = 500,
                            Message = "Lỗi khi tạo tài khoản"
                        };
                    }

                    return new RegisterRespone
                    {
                        HttpStatusCode = 200,
                        Message = $"Đăng ký thành công! ID: {newPlayerId}"
                    };
                }
            }
            catch
            {
                return new RegisterRespone
                {
                    HttpStatusCode = 400,
                    Message = "Có lỗi xảy ra"
                };
            }
        }
    }
}
