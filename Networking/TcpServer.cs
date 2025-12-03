using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Utils.Loggings;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Crimson_Knight_Server.Networking
{
    public abstract class TcpServer
    {
        protected ConcurrentDictionary<int, Player> sessions = new ConcurrentDictionary<int, Player>();
        protected TcpListener listener;
        protected Thread acceptThread;
        protected volatile bool isRunning;



        protected void AcceptClientLoop()
        {
            try
            {
                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Player session = new Player(-1);
                    session.InitNetwork(client);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError("[TcpServer] loi trong AcceptClientLoop " + ex.Message);
            }
        }


        public bool AddSession(Player session)
        {
            return sessions.TryAdd(session.PlayerId, session);
        }

        public bool RemoveSession(Player session)
        {
            return sessions.TryRemove(session.PlayerId, out Player s);
        }


        public void SendOthersInMap(Message msg, Player session)
        {
            foreach (var item in session.MapCur.Players)
            {
                if (item.PlayerId != session.PlayerId)
                {
                    item.SendMessage(msg);
                }
            }
        }


        public void SendAll(Message msg)
        {
            foreach (var item in this.sessions)
            {
                var s = item.Value;
                s.SendMessage(msg);
            }
        }

        public Player GetPlayerById(int playerId)
        {
            if (sessions.TryGetValue(playerId, out Player player))
            {
                return player;
            }
            return null;
        }
    }
}
