using Crimson_Knight_Server.DataAccessLayer;
using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
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
                if (!LoadData())
                {
                    return;
                }
                SetUpGame();
                RunTcpServer();
                RunGameLoop();
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[TcpServer] Không thể khởi động: {ex.Message}");
            }
        }

        private void RunTcpServer()
        {
            listener = new TcpListener(IPAddress.Any, ServerSetting.PORT_TCP);
            isRunning = true;

            acceptThread = new Thread(AcceptClientLoop);
            acceptThread.IsBackground = true;

            listener.Start();
            acceptThread.Start();

            ConsoleLogging.LogInfor($"[TcpServer] Bắt đầu lắng nghe trên port {ServerSetting.PORT_TCP}...");
        }


        private bool LoadData()
        {
            bool flag = true;
            try
            {
                TemplateManager.LoadTemplate();
                LoadDB();
            }
            catch (Exception ex) 
            {
                ConsoleLogging.LogError($"[LoadData] Load loi: {ex.Message}");
                flag = false;
            }
            return flag;
        }

        private void LoadDB()
        {

        }

        private void SetUpGame()
        {
            //map
            foreach(var item in TemplateManager.MapTemplates)
            {
                MapManager.Maps.Add(new Map(item));
            }
            TemplateManager.MapTemplates.Clear();
            MapManager.DepartTemplates = TemplateManager.DepartTemplates;
        }


        private void RunGameLoop()
        {
            new Thread(() =>
            {
                const int loopInterval = 20;
                while (isRunning)
                {
                    long start = SystemUtil.CurrentTimeMillis();

                    CheckPlayerEnterOrExitMap();

                    try
                    {
                        foreach (var item in MapManager.Maps)
                        {
                            item.UpdateMap();
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogging.LogError($"[RunGameLoop] Load loi: {ex.Message}");
                    }

                    long elapsed = SystemUtil.CurrentTimeMillis() - start;
                    int sleepTime = (int)(loopInterval - elapsed);
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }


                    //ConsoleLogging.LogInfor(sessions.Count + "");
                }
            }).Start();

        }


        private void CheckPlayerEnterOrExitMap()
        {
            try
            {
                while (MapManager.PlayerEnterOrExitmap.TryDequeue(out var item))
                {
                    bool isEnterMap = item.Item3;
                    if (isEnterMap)
                    {
                        if (item.Item2.MapCur != null)
                        {
                            ServerMessageSender.ExitMap(item.Item2);
                            item.Item1.Players.Remove(item.Item2);
                        }

                        item.Item1.Players.Add(item.Item2);
                        item.Item2.MapCur = item.Item1;
                        ServerMessageSender.EnterMap(item.Item2);
                        ServerMessageSender.OtherPlayersInMap(item.Item2);
                        ServerMessageSender.MonstersInMap(item.Item2);
                        ServerMessageSender.NpcsInMap(item.Item2);
                    }
                    else
                    {
                        ServerMessageSender.ExitMap(item.Item2);
                        item.Item1.Players.Remove(item.Item2);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[CheckPlayerEnterOrExitMap] Load loi: {ex.Message}");
            }
        }
    }
}
