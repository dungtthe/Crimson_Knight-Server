using Crimson_Knight_Server.DataAccessLayer;
using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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
                RunBackGroundService();
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[TcpServer] Không thể khởi động: {ex.Message}");
            }
        }


        public readonly ConcurrentQueue<Tuple<Player,short,short, short>> SaveDataMsgs = new();

        private void RunBackGroundService()
        {
            new Thread(() =>
            {
                while (isRunning)
                {
                    while(SaveDataMsgs.TryDequeue(out var item))
                    {
                        PlayerService.SaveData(item.Item1, item.Item2, item.Item3, item.Item4);
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
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
            foreach (var item in TemplateManager.MapTemplates)
            {
                MapManager.Maps.Add(new Map(item));
            }
            MapManager.DepartTemplates = TemplateManager.DepartTemplates;
        }


        public ConcurrentQueue<Player> RequestEnterPhobans = new ConcurrentQueue<Player>();
        private void RunGameLoop()
        {
            new Thread(() =>
            {
                const int loopInterval = 20;
                while (isRunning)
                {
                    long start = SystemUtil.CurrentTimeMillis();

                    HandleRequestEnterPhoBan();

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

        private void HandleRequestEnterPhoBan()
        {
            while (this.RequestEnterPhobans.TryDequeue(out Player p))
            {
                try
                {
                    if(p == null || p.IsDie())
                    {
                        continue;
                    }
                    Map mapPhoBan = null;
                    short xEnter = 0;
                    short yEnter = 0;
                    foreach(var map in MapManager.Maps)
                    {
                        MapTemplate template = TemplateManager.MapTemplates[map.Id];
                        if(template.IsPhoBan && map.Players.Count == 0)
                        {
                            mapPhoBan = map;
                            xEnter = template.XEnter;
                            yEnter = template.YEnter;
                            break;
                        }
                    }
                    if(mapPhoBan != null)
                    {
                        mapPhoBan.SetUp();
                        MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool, short, short>(mapPhoBan, p, true, xEnter, yEnter));
                    }
                    else
                    {
                        NpcService.ShowDialogOk("Phó bản đang quá tải, hãy thử lại sau ít phút.", p);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    ConsoleLogging.LogError($"[HandleRequestEnterPhoBan] Load loi: {ex.Message}");
                }
            }
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
                            item.Item2.MapCur.Players.Remove(item.Item2);
                        }
                        item.Item2.X = item.Item4;
                        item.Item2.Y = item.Item5;
                        item.Item1.Players.Add(item.Item2);
                        item.Item2.MapCur = item.Item1;
                        ServerMessageSender.EnterMap(item.Item2);
                        ServerMessageSender.OtherPlayersInMap(item.Item2);
                        ServerMessageSender.MonstersInMap(item.Item2);
                        ServerMessageSender.NpcsInMap(item.Item2);
                        ServerMessageSender.SendWearingItems(item.Item2);
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
