using Crimson_Knight_Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server
{
    public class ServerManager : TcpServer
    {
        private static ServerManager ins;
        public static ServerManager GI()
        {
            if (ins == null)
            {
                ins = new ServerManager();
            }
            return ins;
        }
    }
}
