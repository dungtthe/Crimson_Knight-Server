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
        private ConcurrentDictionary<int, Player> sessions = new ConcurrentDictionary<int, Player>();
        protected TcpListener listener;
        protected Thread acceptThread;
        protected volatile bool isRunning;
        private int nextPlayerId = 0; //tam thoi nhu nay



        protected void AcceptClientLoop()
        {
            try
            {
                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    int playerId = Interlocked.Increment(ref nextPlayerId);
                    ConsoleLogging.LogInfor($"[TcpServer] Chấp nhận kết nối cho tcpClient {playerId}...");
                    Player session = new Player(playerId);
                    session.InitNetwork(client);
                    sessions.TryAdd(playerId, session);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError("[TcpServer] loi trong AcceptClientLoop " + ex.Message);
            }
        }


        public bool RemoveSession(Player session)
        {
            return sessions.TryRemove(session.PlayerId, out Player s);
        }


        public void SendOthers(Message msg, Player session)
        {
            foreach (var item in this.sessions)
            {
                var s = item.Value;
                if (s != session && s.MapCur == session.MapCur)
                {
                    s.SendMessage(msg);
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
    }
}
