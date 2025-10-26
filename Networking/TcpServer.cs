using Crimson_Knight_Server.Utils.Loggings;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Crimson_Knight_Server.Networking
{
    public static class TcpServer
    {
        private static ConcurrentDictionary<int, Session> sessions = new ConcurrentDictionary<int, Session>();

        private static TcpListener listener;
        private static Thread acceptThread;
        private static volatile bool isRunning;
        private static int nextPlayerId = 0; //tam thoi nhu nay

        public static void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, ServerSetting.PORT_TCP);
                isRunning = true;

                acceptThread = new Thread(AcceptClientLoop);
                acceptThread.IsBackground = true;

                listener.Start();
                acceptThread.Start();

                ConsoleLogging.LogInfor($"[TcpServer] Bắt đầu lắng nghe trên port {ServerSetting.PORT_TCP}...");
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[TcpServer] Không thể khởi động: {ex.Message}");
            }
        }

        private static void AcceptClientLoop()
        {
            try
            {
                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    int playerId = Interlocked.Increment(ref nextPlayerId);
                    ConsoleLogging.LogInfor($"[TcpServer] Chấp nhận kết nối cho tcpClient {playerId}...");
                    Session session = new Session(playerId, client);
                    sessions.TryAdd(playerId, session);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError("[TcpServer] loi trong AcceptClientLoop " + ex.Message);
            }
        }


        public static bool RemoveSession(Session session)
        {
            return sessions.TryRemove(session.PlayerId,out Session s);
        }
    }
}
