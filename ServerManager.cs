using Crimson_Knight_Server.DataAccessLayer;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public void Start()
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
                HandlerLoop();
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[TcpServer] Không thể khởi động: {ex.Message}");
            }
        }

        private void HandlerLoop()
        {
            new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        //test phat
                        Thread.Sleep(1000);
                        PlayerRepository playerRepository = new PlayerRepository();
                        playerRepository.GetPlayerById(1);
                    }
                    catch
                    {

                    }
                    Thread.Sleep(10);
                }
            }).Start();

        }
    }
}
