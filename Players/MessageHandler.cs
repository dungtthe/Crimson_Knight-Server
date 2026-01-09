using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Maps.MessageMap.Attack;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class MessageHandler
    {
        private Player session;
        public MessageHandler(Player session)
        {
            this.session = session;
        }
        public void HandleMessage(Message msg)
        {
            if (msg.Id == MessageId.CLIENT_LOGIN)
            {
                string token = msg.ReadString();
                string[] s = token.Split('.');
                int id = int.Parse(s[0]);

                //check lai
                if (!PlayerService.SetupPlayer(session, id))
                {
                    session.Close();
                }
                msg.Close();
                ServerMessageSender.Login(session);
                session.FinalSetup();
                return;
            }
            if (session.Id == -1)
            {
                ConsoleLogging.LogWarning("Chưa đăng nhập mà gửi message khác LOGIN");
                msg.Close();
                return;
            }
            switch (msg.Id)
            {
                case MessageId.CLIENT_PLAYER_MOVE:
                    {
                        int x = msg.ReadInt();
                        int y = msg.ReadInt();
                        session.X = (short)x;
                        session.Y = (short)y;
                        ConsoleLogging.LogInfor($"[Player {session.Id}] Move to ({x},{y})");
                        msg.Close();
                        ServerMessageSender.PlayerMove(this.session, (short)x,(short)y);
                        break;
                    }
                case MessageId.CLIENT_ENTER_MAP:
                    {
                        //short departId = msg.ReadShort();
                        //if (session.MapCur != null)
                        //{
                        //    var mapCur = MapManager.Maps[session.MapCur.Id];
                        //    mapCur.BusPlayerExitMap.Enqueue(session);
                        //}

                        //var depart = MapManager.DepartTemplates[departId];
                        //var mapEnter = MapManager.Maps[depart.MapEnterId];
                        //session.X = depart.XEnter;
                        //session.Y = depart.YEnter;
                        //mapEnter.BusPlayerEnterMap.Enqueue(session);
                        //msg.Close();


                        short departId = msg.ReadShort();
                        msg.Close();
                        var depart = MapManager.DepartTemplates[departId];
                        var mapEnter = MapManager.Maps[depart.MapEnterId];
                        session.X = depart.XEnter;
                        session.Y = depart.YEnter;
                        MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool>(mapEnter, session, true));
                        break;
                    }
                case MessageId.CLIENT_SHOW_MENU:
                    {
                        int npcId = msg.ReadInt();
                        msg.Close();
                        NpcService.ShowMenu(npcId, session);
                        break;
                    }
                case MessageId.CLIENT_SELECT_MENU_ITEM:
                    {
                        int npcId = msg.ReadInt();
                        byte menuItemId = msg.ReadByte();
                        msg.Close();
                        NpcService.SelectMenuItem(npcId, menuItemId, session);
                        break;
                    }
                case MessageId.CLIENT_PLAYER_ATTACK:
                    {
                        int skillUsedId = msg.ReadInt();
                        byte size = msg.ReadByte();
                        bool[] isPlayers = new bool[size];
                        int[] targetIds = new int[size];
                        for (int i = 0;i<size; i++)
                        {
                           isPlayers[i] = msg.ReadBool();
                        }
                        for (int i = 0; i < size; i++)
                        {
                            targetIds[i] = msg.ReadInt();
                        }
                        msg.Close();
                        if(isPlayers.Length == 0 || isPlayers.Length != targetIds.Length)
                        {
                            return;
                        }
                        session.MapCur?.AttackMessages.Add(new AttackMessage()
                        {
                            PlayerSenderId = session.Id,
                            SkillUseId = skillUsedId,
                            IsPlayers = isPlayers,
                            TargetIds = targetIds
                        });
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


    }
}
