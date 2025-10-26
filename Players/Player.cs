using Crimson_Knight_Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class Player : Session
    {
        public Player(int playerId, TcpClient client) : base(playerId, client)
        {

        }
    }
}
