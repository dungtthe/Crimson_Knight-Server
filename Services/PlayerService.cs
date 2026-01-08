using Crimson_Knight_Server.DataAccessLayer.Models;
using Crimson_Knight_Server.DataAccessLayer.Repositories;
using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Services.Dtos;
using Crimson_Knight_Server.Services.Mappers;
using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    if (SystemUtil.CurrentTimeMillis() - timeMillis > 60000)
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
            player.SetUpStats(playerModel.Stats);
            player.SetUpSkills(playerModel.Skills);
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
            MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool>(map,player,true));
        }
    }
}
