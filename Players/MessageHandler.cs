using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (msg.Id == MessageId.LOGIN)
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

                Message msg2 = new Message(MessageId.LOGIN);
                msg2.WriteInt(this.session.PlayerId);
                msg2.WriteString(this.session.Name);
                session.SendMessage(msg2);
                msg2.Close();
                return;
            }
            if (session.PlayerId == -1)
            {
                ConsoleLogging.LogWarning("Chưa đăng nhập mà gửi message khác LOGIN");
                msg.Close();
                return;
            }
            switch (msg.Id)
            {
                case MessageId.PLAYER_MOVE:
                    {
                        int x = msg.ReadInt();
                        int y = msg.ReadInt();
                        session.X = (short)x;
                        session.Y = (short)y;
                        ConsoleLogging.LogInfor($"[Player {session.PlayerId}] Move to ({x},{y})");
                        msg.Close();
                        session.BroadcastMove();
                        break;
                    }
                case MessageId.PLAYER_ENTER_MAP:
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
                default:
                    {
                        break;
                    }
            }
        }

       
    }
}
