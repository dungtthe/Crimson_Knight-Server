using Crimson_Knight_Server.Networking;
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
        private Session session;
        public MessageHandler(Session session)
        {
            this.session = session;
        }
        public void HandleMessage(Message msg)
        {
            switch (msg.Id)
            {
                case MessageId.OK:
                    {
                        break;
                    }
                case MessageId.PLAYER_MOVE:
                    {
                        int x = msg.ReadInt();
                        int y = msg.ReadInt();
                        ConsoleLogging.LogInfor("Player: " + session.PlayerId + " di chuyển tới vị trí: " + x + ", " + y);
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
