using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Crimson_Knight_Server.Players
{
    public class Player : BaseObject
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
                ConsoleLogging.LogError($"[Client {Id}] Lỗi ReceiveLoop: {ex.Message}");
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
                    ConsoleLogging.LogError($"[Client {Id}] Lỗi SendMessage: {ex.Message}");
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
                if (this.Id != -1)
                {
                    //this.MapCur?.BusPlayerExitMap.Enqueue(this);
                    //ServerManager.GI().RemoveSession(this);
                    MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool>(this.MapCur, this, false));
                    ServerManager.GI().RemoveSession(this);
                    ConsoleLogging.LogWarning($"[Client {Id}] Đã đóng kết nối.");
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
        public string Name { get; set; }
        public int CurrentMp { get; set; }
        public short Level { get; set; }

        public Map MapCur;
        public List<Skill> Skills;
        public long Exp { get; set; }

        public Player(int playerId) : base(playerId)
        {
        }

        public void SetUpStats(string data)
        {
            Stats = Helpers.DeserializeStats(data);
        }
        public void SetUpSkills(string data)
        {
            byte size = 1;
            Skills = new List<Skill>();
            string[] s1 = data.Split('_');
            for (int i = 0; i < size; i++)
            {
                string[] s2 = s1[i].Split('.');
                int templateId = int.Parse(s2[0]);
                byte variantId = byte.Parse(s2[1]);
                var skill = new Skill(templateId, variantId, this.ClassType);
                Skills.Add(skill);
            }
        }
        public void FinalSetup()
        {
            this.CurrentHp = this.GetMaxHp();
            this.CurrentMp = this.GetMaxMp();
            this.SendPlayerBaseInfo();
            this.SendPlayerSkillInfo();
        }

        public ClassType ClassType;
        public void SetId(int id)
        {
            this.Id = id;
        }
        public virtual int GetMaxMp()
        {
            int mp = Helpers.GetStatValue(this.Stats, StatId.MP);
            int percentMp = Helpers.GetStatValue(this.Stats, StatId.PERCENT_MP);

            return mp + (int)((long)mp * percentMp / 10000);
        }


        public override bool IsMonster()
        {
            return false;
        }

        public override bool IsPlayer()
        {
            return true;
        }

      

        #region msg
        public void BroadcastEnterMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.SERVER_OTHER_PLAYER_ENTER_MAP);
                msg.WriteInt(Id);
                msg.WriteString(Name);
                msg.WriteShort(X);
                msg.WriteShort(Y);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();

                foreach (var p in MapCur.Players)
                {
                    if (p.Id != this.Id)
                    {
                        p.SendOtherPlayerBaseInfo(this);
                    }
                }
            }
        }

        public void BroadcastMove()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.SERVER_OTHER_PLAYER_MOVE);
                msg.WriteInt(Id);
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
                Message msg = new Message(MessageId.SERVER_ENTER_MAP);
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
                Message msg = new Message(MessageId.SERVER_OTHER_PLAYER_EXIT_MAP);
                msg.WriteInt(Id);
                ServerManager.GI().SendOthersInMap(msg, this);
                msg.Close();
            }
        }

        public void SendMonstersInMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.SERVER_MONSTERS_IN_MAP);
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

                foreach (var item in MapCur.Monsters)
                {
                    item.SendMonsterBaseInfo(this);
                }
            }
        }

        public void SendOtherPlayersInMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.SERVER_OTHERPLAYERS_IN_MAP);
                //other players
                msg.WriteShort((short)this.MapCur.Players.Count);
                foreach (var other in this.MapCur.Players)
                {
                    msg.WriteInt(other.Id);
                    msg.WriteString(other.Name);
                    msg.WriteByte((byte)other.ClassType);
                    msg.WriteShort(other.X);
                    msg.WriteShort(other.Y);
                }
                SendMessage(msg);
                msg.Close();
                ConsoleLogging.LogInfor($"[Player {Id}] Đã gửi otherplayer {this.MapCur.Players.Count} trong map.");
                //
                foreach (var other in this.MapCur.Players)
                {
                    if(other.Id == this.Id)
                    {
                        continue;
                    }
                    SendOtherPlayerBaseInfo(other);
                }
            }
        }

        public void SendNpcsInMap()
        {
            if (MapCur != null)
            {
                Message msg = new Message(MessageId.SERVER_NPCS_IN_MAP);
                msg.WriteShort((short)MapCur.Npcs.Count);
                for (int i = 0; i < MapCur.Npcs.Count; i++)
                {
                    var item = MapCur.Npcs[i];
                    msg.WriteInt(item.Template.Id);
                    msg.WriteShort(item.X);
                    msg.WriteShort(item.Y);
                }
                SendMessage(msg);
                msg.Close();
            }
        }


        public void SendPlayerBaseInfo()
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_BASE_INFO);
            //base
            msg.WriteInt(Id);
            msg.WriteString(Name);
            msg.WriteShort(Level);
            msg.WriteLong(Exp);
            msg.WriteInt(CurrentHp);
            msg.WriteInt(GetMaxHp());
            msg.WriteInt(CurrentMp);
            msg.WriteInt(GetMaxMp());
            SendMessage(msg);
            msg.Close();
        }

        public void SendPlayerSkillInfo()
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_SKILL_INFO);
            msg.WriteByte((byte)Skills.Count);
            foreach (var item in Skills)
            {
                msg.WriteInt(item.TemplateId);
                msg.WriteByte(item.VariantId);
            }
            SendMessage(msg);
            msg.Close();
        }

        public void SendOtherPlayerBaseInfo(Player other)
        {
            Message msg = new Message(MessageId.SERVER_OTHER_PLAYER_BASE_INFO);
            msg.WriteInt(other.Id);
            msg.WriteInt(other.CurrentHp);
            msg.WriteInt(other.GetMaxHp());
            msg.WriteShort(other.Level);
            SendMessage(msg);
            msg.Close();
        }
       
        #endregion

    }

}
