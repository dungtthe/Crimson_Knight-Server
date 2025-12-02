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
                Player player = PlayerService.SetupPlayer(session,id);
                if (player == null)
                {
                    session.Close();
                }
                msg.Close();

                PlayerEnterGame();
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
                        msg.Close();
                        ConsoleLogging.LogInfor("Player: " + session.PlayerId + " di chuyển tới vị trí: " + x + ", " + y);
                        Message msgSend = new Message(MessageId.OTHER_PLAYER_MOVE);
                        msgSend.WriteInt(session.PlayerId);
                        msgSend.WriteInt(x);
                        msgSend.WriteInt(y);
                        ServerManager.GI().SendOthers(msgSend, session);
                        msgSend.Close();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void PlayerEnterGame()
        {

        }
    }
}
