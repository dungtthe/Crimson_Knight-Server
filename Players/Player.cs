using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
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
                    MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool,short,short>(this.MapCur, this, false,-1,-1));
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
        public Gender Gender { get; set; }

        public readonly BaseItem[] InventoryItems = new BaseItem[48];
        public readonly ItemEquipment[] WearingItems = new ItemEquipment[3];

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
        public void SetUpItem(string inventoryItems, string wearingItems)
        {
            JsonElement[] invens = JsonSerializer.Deserialize<JsonElement[]>(inventoryItems);

            for (int i = 0; i < invens.Length; i++)
            {
                if (invens[i].ValueKind != JsonValueKind.Null)
                {
                    JsonElement item = invens[i];
                    ItemType type = (ItemType)item[0].GetByte();
                    JsonElement data = item[1];
                    switch (type)
                    {
                        case ItemType.Equipment:
                            InventoryItems[i] = ItemEquipment.Create(data);
                            break;

                        case ItemType.Consumable:
                            InventoryItems[i] = ItemConsumable.Create(data);
                            break;

                        case ItemType.Material:
                            InventoryItems[i] = ItemMaterial.Create(data);
                            break;
                    }
                }    
            }

            JsonElement[] wears = JsonSerializer.Deserialize<JsonElement[]>(wearingItems);

            for (int i = 0; i < wears.Length; i++)
            {
                if (wears[i].ValueKind != JsonValueKind.Null)
                {
                    WearingItems[i] = ItemEquipment.Create(wears[i]);
                }
            }
        }
        public void FinalSetup()
        {
            this.CurrentHp = this.GetMaxHp();
            this.CurrentMp = this.GetMaxMp();
            ServerMessageSender.PlayerBaseInfo(this, true);
            ServerMessageSender.SendPlayerSkillInfo(this);
            ServerMessageSender.SendWearingItems(this);
            ServerMessageSender.SendInventoryItems(this);
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


        protected override void CheckDie()
        {

        }

        public override bool IsPlayer()
        {
            return true;
        }

        private bool test = false;
        public override void Update()
        {
            //if (test)
            //{
            //    return;
            //}
            //test = true;

            //ItemConsumable hp = new ItemConsumable(0, 5546);
            //InventoryItems[0] = hp;
            //ItemConsumable mp = new ItemConsumable(1, 65);
            //InventoryItems[1] = mp;

            //ItemMaterial da = new ItemMaterial(0, 8855);
            //InventoryItems[2] = da;

            //ItemEquipment vukhi = new ItemEquipment(Helpers.GenerateId(), 0);
            //InventoryItems[3] = vukhi;
            //ItemEquipment ao = new ItemEquipment(Helpers.GenerateId(), 4);
            //InventoryItems[4] = ao;
            //ItemEquipment quan = new ItemEquipment(Helpers.GenerateId(), 6);
            //InventoryItems[5] = quan;

            //WearingItems[0] = vukhi;
            //WearingItems[1] = ao;
            //WearingItems[2] = quan;
            //PlayerService.SaveData(this);
        }

        public int GetAvailableInventory()
        {
            for(int i = 0; i < InventoryItems.Length; i++)
            {
                if(InventoryItems[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetQuantityItem_ConsuOrMaterial(int idTemplate, ItemType type)
        {
            if(type == ItemType.Equipment)
            {
                return 0;
            }
            foreach(var item in  InventoryItems)
            {
                if(item.TemplateId == idTemplate)
                {
                    if (item.GetItemType() == ItemType.Consumable)
                    {
                        return ((ItemConsumable)item).Quantity;
                    }
                    else if (item.GetItemType() == ItemType.Material)
                    {
                        return ((ItemMaterial)item).Quantity;
                    }
                }
            }
            return 0;
        }

        public BaseItem GetItemConsuOrMate(int idTemplate, ItemType type)
        {
            foreach(var item in InventoryItems)
            {
                if(item.TemplateId == idTemplate && item.GetItemType() == type)
                {
                    return item;
                }
            }
            return null;
        }
    }

}
