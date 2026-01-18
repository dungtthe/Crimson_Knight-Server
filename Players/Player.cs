using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Players.MessagePlayer;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Templates.Shops;
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
using System.Xml.Linq;
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
                    MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool, short, short>(this.MapCur, this, false, -1, -1));
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

        public readonly ConcurrentQueue<UseItemMsg> UseItemMsgs = new ConcurrentQueue<UseItemMsg>();
        public readonly ConcurrentQueue<Tuple<ItemShop, int>> BuyItems = new ConcurrentQueue<Tuple<ItemShop, int>>();

        public string Name { get; set; }

        private int _currentMp;
        public int CurrentMp
        {
            get => _currentMp;
            set
            {
                if (_currentMp == value) return;
                _currentMp = Math.Max(0, value);
                int maxMp = GetMaxMp();
                if (_currentMp > maxMp)
                {
                    _currentMp = maxMp;
                }
            }
        }
        public short Level { get; set; }
        public PkType PkType { get; set; }

        public Map MapCur;
        public List<Skill> Skills;
        public long Exp { get; set; }
        public Gender Gender { get; set; }
        public Quest Quest { get; set; }

        public readonly BaseItem[] InventoryItems = new BaseItem[48];
        public readonly ItemEquipment[] WearingItems = new ItemEquipment[4];

        public long Gold { get; set; }

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
            Skills = new List<Skill>();
            string[] s1 = data.Split('_');
            for (int i = 0; i < s1.Length; i++)
            {
                string[] s2 = s1[i].Split('.');
                int templateId = int.Parse(s2[0]);
                sbyte variantId = sbyte.Parse(s2[1]);
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

        public void SetUpQuest(string quest)
        {
            if (quest == null)
            {
                this.Quest = null;
            }
            else
            {
                string[] s = quest.Split(".");
                int id = int.Parse(s[0]);
                int quantity = int.Parse(s[1]);
                byte state = byte.Parse(s[2]);
                this.Quest = new Quest()
                {
                    Id = id,
                    QuantityCur = quantity,
                    QuestState = (QuestState)state,
                };
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
            ServerMessageSender.SendQuest(this);
        }

        public ClassType ClassType;
        public void SetId(int id)
        {
            this.Id = id;
        }



        protected override void CheckDie()
        {

        }

        public override bool IsPlayer()
        {
            return true;
        }

        public override void Update()
        {
            UpdateUseItemMsgs();
            HandleBuyItemMsgs();
        }

        private void HandleBuyItemMsgs()
        {
            while (BuyItems.TryDequeue(out var item))
            {
                ItemShop itemShop = item.Item1;
                int quantity = item.Item2;
                if (this.Gold < itemShop.Price * quantity)
                {
                    ServerMessageSender.CenterNotificationView(this, "Không đủ vàng");
                    continue;
                }

                int quantityAvai = GetAvailableSlotInventory();
                if (itemShop.ItemType == ItemType.Equipment)
                {
                    if (quantityAvai < quantity)
                    {
                        ServerMessageSender.CenterNotificationView(this, "Không đủ hành trang");
                        continue;
                    }
                    for (int i = 0; i < quantity; i++)
                    {
                        ItemEquipment itemEquipment = new ItemEquipment(Helpers.GenerateId(), itemShop.IdItem);
                        AddItem(itemEquipment);
                    }
                }
                else if (itemShop.ItemType == ItemType.Consumable)
                {
                    var itemConsu = GetItemConsuOrMate(itemShop.IdItem, ItemType.Consumable);
                    if (itemConsu == null)
                    {
                        if (quantityAvai == 0)
                        {
                            ServerMessageSender.CenterNotificationView(this, "Không đủ hành trang");
                            continue;
                        }
                        else
                        {
                            ItemConsumable itemConsumable = new ItemConsumable(itemShop.IdItem, quantity);
                            AddItem(itemConsumable);
                        }
                    }
                    else
                    {
                        ItemConsumable itemConsumable = itemConsu as ItemConsumable;
                        itemConsumable.Quantity += quantity;
                    }
                }
                else
                {
                    var itemMate = GetItemConsuOrMate(itemShop.IdItem, ItemType.Material);
                    if (itemMate == null)
                    {

                        if (quantityAvai == 0)
                        {
                            ServerMessageSender.CenterNotificationView(this, "Không đủ hành trang");
                            continue;
                        }
                        else
                        {
                            ItemMaterial itemMaterial = new ItemMaterial(itemShop.IdItem, quantity);
                            AddItem(itemMaterial);
                        }
                    }
                    else
                    {
                        ItemMaterial itemMaterial = itemMate as ItemMaterial;
                        itemMaterial.Quantity += quantity;
                    }
                }

                Gold -= quantity * itemShop.Price;
                ServerMessageSender.CenterNotificationView(this, $"Bạn nhận được {quantity} {itemShop.GetName()}");
                ServerMessageSender.SendInventoryItems(this);
                ServerMessageSender.PlayerInfoGold(this);
            }
        }

        private long startTimeUseHp = 0;
        private long startTimeUseMp = 0;
        private void UpdateUseItemMsgs()
        {
            while (UseItemMsgs.TryDequeue(out var msg))
            {
                try
                {
                    string idItem = msg.ItemId;
                    ItemType itemType = msg.ItemType;
                    if (itemType == ItemType.Consumable)
                    {
                        int templateId = int.Parse(idItem);
                        int quantity = GetQuantityItem_ConsuOrMaterial(templateId, itemType);
                        if (quantity == 0)
                        {
                            string name = TemplateManager.ItemConsumableTemplates[templateId].Name;
                            ServerMessageSender.CenterNotificationView(this, $"Đã hết {name}");
                        }
                        else
                        {
                            HandleUseItemConsumable(templateId);
                        }
                    }
                    else if (itemType == ItemType.Material)
                    {
                        ServerMessageSender.CenterNotificationView(this, "Vật phẩm này chỉ được dùng để nâng cấp");
                    }
                    else
                    {
                        var itemEq = GetItemEquipment(idItem);
                        if (itemEq == null)
                        {
                            ServerMessageSender.CenterNotificationView(this, "Không tìm thấy vật phẩm");
                            ServerMessageSender.SendInventoryItems(this);
                        }
                        else
                        {
                            HandleUseItemEquipment(itemEq);
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleLogging.LogError($"Lỗi ở UpdateUseItemMsgs {e.Message}");
                }
            }


            void HandleUseItemConsumable(int templateId)
            {
                BaseItem baseItem = GetItemConsuOrMate(templateId, ItemType.Consumable);
                if (baseItem == null)
                {
                    return;
                }

                if (this.Level < TemplateManager.ItemConsumableTemplates[templateId].LevelRequire)
                {
                    ServerMessageSender.CenterNotificationView(this, "Không đủ level để dùng vật phẩm này");
                    return;
                }

                if (templateId == 0 || templateId == 2)//hp
                {
                    if (SystemUtil.CurrentTimeMillis() - startTimeUseHp < TemplateManager.ItemConsumableTemplates[templateId].Cooldown)
                    {
                        ServerMessageSender.CenterNotificationView(this, "Thao tác quá nhanh");
                        return;
                    }
                    startTimeUseHp = SystemUtil.CurrentTimeMillis();
                }
                else if (templateId == 1 || templateId == 3)//mp
                {
                    if (SystemUtil.CurrentTimeMillis() - startTimeUseMp < TemplateManager.ItemConsumableTemplates[templateId].Cooldown)
                    {
                        ServerMessageSender.CenterNotificationView(this, "Thao tác quá nhanh");
                        return;
                    }
                    startTimeUseMp = SystemUtil.CurrentTimeMillis();
                }

                ((ItemConsumable)baseItem).Quantity -= 1;
                if (templateId == 0 || templateId == 2)//hp
                {
                    CurrentHp += (int)TemplateManager.ItemConsumableTemplates[templateId].Value;
                }
                else if (templateId == 1 || templateId == 3)//mp
                {
                    CurrentMp += (int)TemplateManager.ItemConsumableTemplates[templateId].Value;
                }
                if (((ItemConsumable)baseItem).Quantity == 0)
                {
                    RemoveItem(baseItem);
                }
                ServerMessageSender.PlayerBaseInfo(this, true);
                ServerMessageSender.SendInventoryItems(this);
            }

            void HandleUseItemEquipment(ItemEquipment itemUse)
            {
                var template = itemUse.GetTemplate();
                if (template.Gender != Gender.Unisex && template.Gender != this.Gender)
                {
                    ServerMessageSender.CenterNotificationView(this, "Giới tính không phù hợp");
                    return;
                }

                if (template.ClassType != ClassType.NONE && template.ClassType != this.ClassType)
                {
                    ServerMessageSender.CenterNotificationView(this, "Class không phù hợp");
                    return;
                }
                if (template.LevelRequire > this.Level)
                {
                    ServerMessageSender.CenterNotificationView(this, "Không đủ Level");
                    return;
                }
                EquipmentType equipmentType = template.EquipmentType;

                RemoveItem(itemUse);

                var itemWearing = WearingItems[(int)equipmentType];
                if (itemWearing != null)
                {
                    AddItem(itemWearing);
                }
                WearingItems[(int)equipmentType] = itemUse;
                ServerMessageSender.SendInventoryItems(this);
                ServerMessageSender.SendWearingItems(this);
            }
        }

        public int GetAvailableIndexInventory()
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetAvailableSlotInventory()
        {
            int count = 0;
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
                {
                    count++;
                }
            }
            return count;
        }

        public void RemoveItem(BaseItem item)
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == item)
                {
                    InventoryItems[i] = null;
                    return;
                }
            }
        }

        public void AddItem(BaseItem item)
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
                {
                    InventoryItems[i] = item;
                    return;
                }
            }
        }

        public int GetQuantityItem_ConsuOrMaterial(int idTemplate, ItemType type)
        {
            if (type == ItemType.Equipment)
            {
                return 0;
            }
            foreach (var item in InventoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.TemplateId == idTemplate && item.GetItemType() == type)
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
            foreach (var item in InventoryItems)
            {
                if (item == null)
                {
                    continue;
                }
                if (item.TemplateId == idTemplate && item.GetItemType() == type)
                {
                    return item;
                }
            }
            return null;
        }


        public ItemEquipment GetItemEquipment(string iditem)
        {
            foreach (var item in InventoryItems)
            {
                if (item != null && item.GetItemType() == ItemType.Equipment && item.Id == iditem)
                {
                    return (ItemEquipment)item;
                }
            }
            return null;
        }


        public override int GetMaxHp()
        {
            int baseHp = base.GetMaxHp();

            int flatHp = 0;
            int percentHp = 0;

            foreach (var item in WearingItems)
            {
                if (item == null) continue;

                var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;

                flatHp += Helpers.GetStatValue(stats, StatId.HP);
                percentHp += Helpers.GetStatValue(stats, StatId.PERCENT_HP);
            }

            return baseHp
                 + flatHp
                 + (int)((long)baseHp * percentHp / 10000);
        }


        public int GetMaxMp()
        {
            int BaseMp()
            {
                int mp = Helpers.GetStatValue(this.Stats, StatId.MP);
                int percentMp = Helpers.GetStatValue(this.Stats, StatId.PERCENT_MP);

                return mp + (int)((long)mp * percentMp / 10000);
            }

            int baseMp = BaseMp();

            int flatMp = 0;
            int percentMp = 0;

            foreach (var item in WearingItems)
            {
                if (item == null) continue;

                var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;

                flatMp += Helpers.GetStatValue(stats, StatId.MP);
                percentMp += Helpers.GetStatValue(stats, StatId.PERCENT_MP);
            }

            return baseMp
                 + flatMp
                 + (int)((long)baseMp * percentMp / 10000);
        }

        public override int GetAtk()
        {
            int baseAtk = base.GetAtk();

            int flatAtk = 0;
            int percentAtk = 0;

            foreach (var item in WearingItems)
            {
                if (item == null) continue;

                var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;

                flatAtk += Helpers.GetStatValue(stats, StatId.ATK);
                percentAtk += Helpers.GetStatValue(stats, StatId.PERCENT_ATK);
            }

            return baseAtk
                 + flatAtk
                 + (int)((long)baseAtk * percentAtk / 10000);
        }

        public override int GetDef()
        {
            int baseDef = base.GetDef();

            int flatDef = 0;
            int percentDef = 0;

            foreach (var item in WearingItems)
            {
                if (item == null) continue;

                var stats = TemplateManager.ItemEquipmentTemplates[item.TemplateId].Stats;

                flatDef += Helpers.GetStatValue(stats, StatId.DEF);
                percentDef += Helpers.GetStatValue(stats, StatId.PERCENT_DEF);
            }

            return baseDef
                 + flatDef
                 + (int)((long)baseDef * percentDef / 10000);
        }

        public void UpdateExp(int exp)
        {
            if (exp <= 0) return;

            int maxLevel = TemplateManager.Levels.Count - 1;

            if (this.Level >= maxLevel)
                return;

            this.Exp += exp;

            if (this.Exp >= TemplateManager.Levels[maxLevel])
            {
                this.Exp = TemplateManager.Levels[maxLevel];
                this.Level = (short)maxLevel;
                return;
            }

            while (this.Level < maxLevel &&
                   this.Exp >= TemplateManager.Levels[this.Level + 1])
            {
                this.Level++;
                OnLevelUp(this.Level);
            }
            ServerMessageSender.PlayerBaseInfo(this, true);
        }

        private void OnLevelUp(short newLevel)
        {

        }

        public void UpdateGold(int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }
            Gold += quantity;
            ServerMessageSender.PlayerInfoGold(this);
            ServerMessageSender.CenterNotificationView(this, $"Bạn nhận được {quantity} vàng");
        }
    }

}
