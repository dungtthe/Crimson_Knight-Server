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
        public PkType PkType { get; set; }

        public Map MapCur;
        public List<Skill> Skills;
        public long Exp { get; set; }

        public Player(int playerId) : base(playerId)
        {
            this.PkType = PkType.None;
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
            ServerMessageSender.PlayerBaseInfo(this, true);
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

        protected override void CheckDie()
        {

        }

        #region msg

















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

       
        #endregion

    }

}
