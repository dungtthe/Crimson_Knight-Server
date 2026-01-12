using Crimson_Knight_Server.Maps.MessageMap;
using Crimson_Knight_Server.Maps.MessageMap.Attack;
using Crimson_Knight_Server.Monsters;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Npcs;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public class Map
    {
        public ConcurrentBag<AttackMessage> AttackMessages = new ConcurrentBag<AttackMessage>();
        public ConcurrentBag<PickItemMsg> PickItemMessages = new ConcurrentBag<PickItemMsg>();

        public short Id { get; set; }
        public string Name { get; set; }
        public short XEnter { get; set; }
        public short YEnter { get; set; }

        public List<Monster> Monsters = new List<Monster>();
        public List<Npc> Npcs = new List<Npc>();
        public List<Player> Players = new List<Player>();
        private Dictionary<string, ItemPick> itemPicks = new();

        public Player GetPlayerById(int playerId)
        {
            return Players.FirstOrDefault(p => p.Id == playerId);
        }
        public Map(MapTemplate template)
        {
            this.Id = template.Id;
            this.Name = template.Name;
            this.XEnter = template.XEnter;
            this.YEnter = template.YEnter;
            if (template.Monsters != null)
            {
                for (int i = 0; i < template.Monsters.Count; i++)
                {
                    var item = template.Monsters[i];
                    var monster = new Monster(i, item.X, item.Y, TemplateManager.MonsterTemplates[item.TemplateId],this);
                    Monsters.Add(monster);
                }
            }
            if (template.Npcs != null)
            {
                for (int i = 0; i < template.Npcs.Count; i++)
                {
                    var item = template.Npcs[i];
                    var npc = new Npc(item.X, item.Y, TemplateManager.NpcTemplates[item.TemplateId]);
                    Npcs.Add(npc);
                }
            }
        }


        public void UpdateMap()
        {
            UpdatePickItems();
            HandleAttackMessages();
            UpdateMonsters();
            UpdatePlayers();
        }

        private void UpdatePickItems()
        {
            UpdatePickItemsMessage();

            List<string> itemremoves = new List<string>();
            foreach(var item in itemPicks.Values)
            {
                if (item.PlayerId == -1)
                {
                    if(SystemUtil.CurrentTimeMillis() - item.StartLeaveTime > 10000)
                    {
                        itemremoves.Add(item.Id);
                    }
                }
                else
                {
                    if (SystemUtil.CurrentTimeMillis() - item.StartLeaveTime > 5000)
                    {
                        item.PlayerId = -1;
                    }
                }
            }
            foreach (var id in itemremoves)
            {
                itemPicks.Remove(id);
                ServerMessageSender.RemoveItemPick(this,id);
            }
        }
        private void UpdatePickItemsMessage()
        {
            while(PickItemMessages.TryTake(out var msg))
            {
                string id = msg.Id;
                Player p = msg.Player;
                if(p != null)
                {
                    string content = "";
                    if(itemPicks.TryGetValue(id, out var item))
                    {
                        if(item.PlayerId == -1 || item.PlayerId == p.Id)
                        {
                            if(item.ItemType == ItemType.Equipment)
                            {
                                int indexBag = p.GetAvailableInventory();
                                if(indexBag == -1)
                                {
                                    content = "Hành trang không đủ chỗ trống";
                                }
                                else
                                {
                                    ItemEquipment itemEquipment = new ItemEquipment(Helpers.GenerateId(), item.TemplateId);
                                    p.InventoryItems[indexBag] = itemEquipment;
                                    ServerMessageSender.PlayerPickItem(p, id, false);
                                    ServerMessageSender.SendInventoryItems(p);
                                    content = "Bạn nhận được " + itemEquipment.GetName();
                                }
                            }
                            else
                            {
                                int quantity = p.GetQuantityItem_ConsuOrMaterial(item.TemplateId, item.ItemType);
                                if(quantity == 0)
                                {
                                    int indexBag = p.GetAvailableInventory();
                                    if (indexBag == -1)
                                    {
                                        content = "Hành trang không đủ chỗ trống";
                                    }
                                    else
                                    {
                                        BaseItem baseItem = null;
                                        if(item.ItemType == ItemType.Consumable)
                                        {
                                            baseItem = new ItemConsumable(item.TemplateId, 1);
                                        }
                                        else
                                        {
                                            baseItem = new ItemMaterial(item.TemplateId, 1);
                                        }
                                        p.InventoryItems[indexBag] = baseItem;
                                        ServerMessageSender.PlayerPickItem(p, id, false);
                                        ServerMessageSender.SendInventoryItems(p);
                                        content = "Bạn nhận được " + baseItem.GetName();
                                    }
                                }
                                else
                                {
                                    BaseItem baseItem = p.GetItemConsuOrMate(item.TemplateId, item.ItemType);
                                    if (baseItem != null)
                                    {
                                        if(baseItem.GetItemType() == ItemType.Consumable)
                                        {
                                            ((ItemConsumable)baseItem).Quantity += 1;
                                        }
                                        else
                                        {
                                            ((ItemMaterial)baseItem).Quantity += 1;
                                        }
                                        ServerMessageSender.PlayerPickItem(p, id, false);
                                        ServerMessageSender.SendInventoryItems(p);
                                        content = "Bạn nhận được " + baseItem.GetName();
                                    }
                                }
                            }
                        }
                        else
                        {
                            content = "Vật phẩm của người khác";
                        }
                    }
                    else
                    {
                        content = "Vật phẩm đã được nhặt bởi người khác";
                        ServerMessageSender.PlayerPickItem(p, id, true);
                    }
                    ServerMessageSender.CenterNotificationView(p, content);
                }
            }
        }

        private void UpdatePlayers()
        {
            foreach(var p in Players)
            {
                p.Update();
            }
        }

        void UpdateMonsters()
        {
            foreach (var monster in Monsters)
            {
                monster.Update();
            }
        }

        private void HandleAttackMessages()
        {
            while (AttackMessages.TryTake(out var attackMessage))
            {
                try
                {
                    Player playerSend = GetPlayerById(attackMessage.PlayerSenderId);
                    if (playerSend == null)
                    {
                        continue;
                    }
                    Skill skillUse = playerSend.Skills[attackMessage.SkillUseId];
                    if (!skillUse.IsLearned || !skillUse.CanAttack(playerSend.CurrentMp))
                    {
                        continue;
                    }
                    skillUse.StartTimeAttack = SystemUtil.CurrentTimeMillis();
                    playerSend.CurrentMp -= skillUse.GetMpLost();

                    int countTarget = 0;
                    List<BaseObject> objTakeDamages = new List<BaseObject>();
                    for (int i = 0; i < attackMessage.TargetIds.Length; i++)
                    {
                        if (countTarget >= skillUse.GetTargetCount())
                        {
                            break;
                        }
                        countTarget++;
                        BaseObject objReceive = null;
                        int objectTargetId = attackMessage.TargetIds[i];
                        bool isPlayer = attackMessage.IsPlayers[i];
                        if (isPlayer)
                        {
                            objReceive = GetPlayerById(objectTargetId);
                            if (objReceive == null || objReceive.IsDie())
                            {
                                continue;
                            }
                            if(playerSend.PkType == ((Player)objReceive).PkType)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            objReceive = Monsters[objectTargetId];
                            if (objReceive.IsDie())
                            {
                                continue;
                            }
                        }
                        int atk = playerSend.GetAtk();
                        var statsSkill = skillUse.GetStats();
                        int flatDmg = Helpers.GetStatValue(statsSkill, StatId.ATK);
                        int percentDmg = Helpers.GetStatValue(statsSkill, StatId.PERCENT_ATK);
                        int dam = atk + flatDmg + (int)((long)atk * percentDmg / 10000);


                        dam = dam - objReceive.GetDef();

                        if (dam < 1)
                        {
                            dam = 1;
                        }
                        objReceive.TakeDamage(dam, playerSend);
                        objTakeDamages.Add(objReceive);

                        foreach (var item in objTakeDamages)
                        {
                            if (item.IsMonster())
                            {
                                ServerMessageSender.MonsterBaseInfo((Monster)item, this);
                            }
                            else
                            {
                                ServerMessageSender.PlayerBaseInfo((Player)item, true);
                            }
                        }
                        SendAttackPlayerInfoMsg(playerSend, skillUse.TemplateId, dam, objTakeDamages);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLogging.LogError("HandleAttackMessages" + ex.Message);
                }
            }
        }

        void SendAttackPlayerInfoMsg(BaseObject attacker, int skillUseId, int dame, List<BaseObject> targets)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_ATTACK);
            msg.WriteInt(attacker.Id);
            msg.WriteInt(skillUseId);
            msg.WriteInt(dame);

            msg.WriteByte((byte)targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                msg.WriteBool(targets[i].IsPlayer());
                msg.WriteInt(targets[i].Id);
            }
            foreach (var p in Players)
            {
                p.SendMessage(msg);
            }
            msg.Close();
        }

        public void DropItem(int idItemTemplate, ItemType itemType, int idPlayerOwner, Monster monster, int quantity = 1)
        {
            string idItemPick = Helpers.GenerateId();
            ItemPick item = new ItemPick(idItemPick, idItemTemplate, itemType, quantity, idPlayerOwner);
            if(itemPicks.TryAdd(idItemPick, item))
            {
                ServerMessageSender.DropItem(this, monster.X, monster.Y, item);
            }
        }
    }
}
