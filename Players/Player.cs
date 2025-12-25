using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Crimson_Knight_Server.Players
{
    public class Player : BaseObject, IPlayerBroadcaster, IPlayerSelfSender
    {
        #region network
        private TcpClient tcpClient;
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private Thread receiveThread;
        private static Thread sendThread;
        private volatile bool isRunning;
        private MessageHandler messageHandler;

        public void InitNetwork(TcpClient client)
        {
            tcpClient = client;
            stream = client.GetStream();
            reader = new BinaryReader(stream, Encoding.UTF8, true);//đảm bảo stream là người close
            writer = new BinaryWriter(stream, Encoding.UTF8, true);//đảm bảo stream là người close
            messageHandler = new MessageHandler(this);
            Start();
        }

        private void Start()
        {
            isRunning = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            try
            {
                while (isRunning)
                {
                    int length = reader.ReadInt32();//co blocking roi nen k phai lo
                    if (length <= 0) continue;
                    byte[] data = reader.ReadBytes(length);
                    Message msg = new Message(data);
                    messageHandler.HandleMessage(msg);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogging.LogError($"[Client {PlayerId}] Lỗi ReceiveLoop: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }


        private object lockSendMessage = new object();
        public void SendMessage(Message msg)
        {
            if (!isRunning) return;
            lock (lockSendMessage)
            {
                try
                {
                    byte[] data = msg.GetData();
                    writer.Write(data.Length);
                    writer.Write(data);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    ConsoleLogging.LogError($"[Client {PlayerId}] Lỗi SendMessage: {ex.Message}");
                    Close();
                }
            }
        }


        private static object lockClose = new object();
        public void Close()
        {
            if (!isRunning) return;
            isRunning = false;

            lock (lockClose)
            {
                if (this.PlayerId != -1)
                {
                    //this.MapCur?.BusPlayerExitMap.Enqueue(this);
                    //ServerManager.GI().RemoveSession(this);
                    MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool>(this.MapCur, this, false));
                    ServerManager.GI().RemoveSession(this);
                    ConsoleLogging.LogWarning($"[Client {PlayerId}] Đã đóng kết nối.");
                }
                else
                {

                }
                try { reader.Close(); } catch { }
                try { writer.Close(); } catch { }
                try { stream.Close(); } catch { }
                try { tcpClient.Close(); } catch { }
            }
        }


        #endregion

        public int PlayerId;
        public string Name;
        public Map MapCur;

        public Player(int playerId) : base(playerId)
        {
            PlayerId = playerId;
        }

        public void SetUpStats(string data)
        {
            Stats = JsonSerializer.Deserialize<Dictionary<StatId, Stat>>(data, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() } // để deserialize enum từ string key
            });
        }


        public ClassType ClassType;
        //stats

        public override int GetMaxHp()
        {

            int statHp = Stats.TryGetValue(StatId.HP, out Stat stat) ? stat.Value : 1;

            if (ClassType == ClassType.CHIEN_BINH)
            {
                return statHp * 20;
            }
            else if (ClassType == ClassType.SAT_THU)
            {
                return statHp * 10;
            }
            else if (ClassType == ClassType.PHAP_SU)
            {
                return statHp * 8;
            }
            else if (ClassType == ClassType.XA_THU)
            {
                return statHp * 12;
            }
            return statHp;
        }

        public int GetMaxMp()
        {
            int statMp = Stats.TryGetValue(StatId.MP, out Stat stat) ? stat.Value : 1;

            if (ClassType == ClassType.CHIEN_BINH)
            {
                return statMp * 5;
            }
            else if (ClassType == ClassType.SAT_THU)
            {
                return statMp * 8;
            }
            else if (ClassType == ClassType.PHAP_SU)
            {
                return statMp * 20;
            }
            else if (ClassType == ClassType.XA_THU)
            {
                return statMp * 15;
            }
            return statMp;
        }
        public override int GetAtk()
        {
            int statATK = Stats.TryGetValue(StatId.ATK, out Stat stat) ? stat.Value : 1;

            if (ClassType == ClassType.CHIEN_BINH)
            {
                return statATK * 3;
            }
            else if (ClassType == ClassType.SAT_THU)
            {
                return statATK * 2;
            }
            else if (ClassType == ClassType.PHAP_SU)
            {
                return statATK * 2;
            }
            else if (ClassType == ClassType.XA_THU)
            {
                return statATK * 2;
            }
            return statATK;
        }

        public override int GetDef()
        {
            int statDEF = Stats.TryGetValue(StatId.DEF, out Stat stat) ? stat.Value : 1;
            if (ClassType == ClassType.CHIEN_BINH)
            {
                return statDEF * 2;
            }
            else if (ClassType == ClassType.SAT_THU)
            {
                return statDEF * 1;
            }
            else if (ClassType == ClassType.PHAP_SU)
            {
                return statDEF * 1;
            }
            else if (ClassType == ClassType.XA_THU)
            {
                return statDEF * 1;
            }
            return statDEF;
        }

        #region msg
        public void BroadcastEnterMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.OTHER_PLAYER_ENTER_MAP);
                msg.WriteInt(PlayerId);
                msg.WriteString(Name);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void BroadcastMove()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.OTHER_PLAYER_MOVE);
                msg.WriteInt(PlayerId);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void SendEnterMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.PLAYER_ENTER_MAP);
                //map
                msg.WriteShort(MapCur.Id);
                msg.WriteString(MapCur.Name);

                //player
                msg.WriteShort(this.X);
                msg.WriteShort(this.Y);

                SendMessage(msg);
                msg.Close();
            }
        }

        public void BroadcastExitMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.OTHER_PLAYER_EXIT_MAP);
                msg.WriteInt(PlayerId);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void SendMonstersInMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.PLAYER_MONSTERS_IN_MAP);
                msg.WriteShort((short)MapCur.Monsters.Count);
                for (int i = 0; i < MapCur.Monsters.Count; i++)
                {
                    var item = MapCur.Monsters[i];
                    msg.WriteInt(item.Template.Id);
                    //
                    msg.WriteInt(item.Id);
                    msg.WriteShort(item.X);
                    msg.WriteShort(item.Y);
                }
                SendMessage(msg);
                msg.Close();
            }
        }

        public void SendOtherPlayersInMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.PLAYER_OTHERPLAYERS_IN_MAP);
                //other players
                msg.WriteShort((short)this.MapCur.Players.Count);
                foreach (var other in this.MapCur.Players)
                {
                    msg.WriteInt(other.PlayerId);
                    msg.WriteString(other.Name);
                    msg.WriteShort(other.X);
                    msg.WriteShort(other.Y);
                }
                SendMessage(msg);
                msg.Close();
            }
        }

        #endregion

    }

}
