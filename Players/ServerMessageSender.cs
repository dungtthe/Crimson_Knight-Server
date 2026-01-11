using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Monsters;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Crimson_Knight_Server.Players
{
    public static class ServerMessageSender
    {
        public static void Login(Player p)
        {
            Message msg = new Message(MessageId.SERVER_LOGIN);
            msg.WriteInt(p.Id);
            msg.WriteString(p.Name);
            msg.WriteByte((byte)p.ClassType);
            msg.WriteByte((byte)p.Gender);
            p.SendMessage(msg);
            msg.Close();
        }

        public static void PlayerMove(Player p, short x, short y)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_MOVE);
            msg.WriteInt(p.Id);
            msg.WriteShort(x);
            msg.WriteShort(y);
            ServerManager.GI().SendOthersInMap(msg, p);
            msg.Close();
        }

        public static void EnterMap(Player p)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("EnterMap: mapcur la null");
                return;
            }
            bool isLoadMap = true;
            Message msg = new Message(MessageId.SERVER_ENTER_MAP);
            msg.WriteBool(isLoadMap);
            msg.WriteShort(p.MapCur.Id);
            msg.WriteString(p.MapCur.Name);
            msg.WriteShort(p.X);
            msg.WriteShort(p.Y);
            p.SendMessage(msg);
            msg.Close();
            ConsoleLogging.LogError($"EnterMap: {p.X} {p.Y}");

            //broadcast
            msg = new Message(MessageId.SERVER_ENTER_MAP);
            msg.WriteBool(!isLoadMap);
            msg.WriteInt(p.Id);
            msg.WriteString(p.Name);
            msg.WriteShort(p.X);
            msg.WriteShort(p.Y);
            msg.WriteByte((byte)p.ClassType);
            msg.WriteByte((byte)p.Gender);
            ServerManager.GI().SendOthersInMap(msg, p);
            msg.Close();
        }

        public static void ExitMap(Player p)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_EXIT_MAP);
            msg.WriteInt(p.Id);
            ServerManager.GI().SendOthersInMap(msg, p);
            msg.Close();
        }


        public static void MonstersInMap(Player p)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("MonstersInMap: mapcur la null");
                return;
            }

            Message msg = new Message(MessageId.SERVER_MONSTERS_IN_MAP);
            msg.WriteShort((short)p.MapCur.Monsters.Count);
            for (int i = 0; i < p.MapCur.Monsters.Count; i++)
            {
                var item = p.MapCur.Monsters[i];
                msg.WriteInt(item.Template.Id);
                //
                msg.WriteInt(item.Id);
                msg.WriteShort(item.X);
                msg.WriteShort(item.Y);
            }
            p.SendMessage(msg);
            msg.Close();

            foreach (var item in p.MapCur.Monsters)
            {
                MonsterBaseInfo(item, p.MapCur);
            }
        }
        public static void NpcsInMap(Player p)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("NpcsInMap: mapcur la null");
                return;
            }

            Message msg = new Message(MessageId.SERVER_NPCS_IN_MAP);
            msg.WriteShort((short)p.MapCur.Npcs.Count);
            for (int i = 0; i < p.MapCur.Npcs.Count; i++)
            {
                var item = p.MapCur.Npcs[i];
                msg.WriteInt(item.Template.Id);
                msg.WriteShort(item.X);
                msg.WriteShort(item.Y);
            }
            p.SendMessage(msg);
            msg.Close();
        }


        public static void OtherPlayersInMap(Player p)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("OtherPlayersInMap: mapcur la null");
                return;
            }

            Message msg = new Message(MessageId.SERVER_OTHERPLAYERS_IN_MAP);
            msg.WriteShort((short)p.MapCur.Players.Count);
            foreach (var other in p.MapCur.Players)
            {
                msg.WriteInt(other.Id);
                msg.WriteString(other.Name);
                msg.WriteByte((byte)other.ClassType);
                msg.WriteShort(other.X);
                msg.WriteShort(other.Y);
                msg.WriteByte((byte)other.Gender);
            }
            p.SendMessage(msg);
            msg.Close();


            foreach (var item in p.MapCur.Players)
            {
                PlayerBaseInfo(item, true);
            }
        }
        public static void PlayerBaseInfo(Player p, bool isSendFull)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("PlayerBaseInfo: mapcur la null");
                isSendFull = false;
            }
            Message msg = new Message(MessageId.SERVER_PLAYER_BASE_INFO);
            //base
            msg.WriteInt(p.Id);
            msg.WriteString(p.Name);
            msg.WriteShort(p.Level);
            msg.WriteLong(p.Exp);
            msg.WriteInt(p.CurrentHp);
            msg.WriteInt(p.GetMaxHp());
            msg.WriteInt(p.CurrentMp);
            msg.WriteInt(p.GetMaxMp());
            if (isSendFull)
            {
                foreach (var item in p.MapCur.Players)
                {
                    item.SendMessage(msg);
                }
            }
            else
            {
                p.SendMessage(msg);
            }
            msg.Close();
            SendPkType(p);
        }

        public static void MonsterBaseInfo(Monster m, Map map)
        {
            Message msg = new Message(MessageId.SERVER_MONSTER_BASE_INFO);
            msg.WriteInt(m.Id);
            msg.WriteInt(m.CurrentHp);
            msg.WriteInt(m.GetMaxHp());
            foreach (var item in map.Players)
            {
                item.SendMessage(msg);
            }
            msg.Close();
        }

        public static void SendPkType(Player p)
        {
            if (p.MapCur == null)
            {
                ConsoleLogging.LogError("SendPkType: mapcur la null");
                return;
            }
            Message msg = new Message(MessageId.SERVER_PLAYER_PKTYPE_INFO);
            msg.WriteInt(p.Id);
            msg.WriteByte((byte)p.PkType);
            foreach (var item in p.MapCur.Players)
            {
                item.SendMessage(msg);
            }
            msg.Close();
        }

        public static void SendPlayerSkillInfo(Player p)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_SKILL_INFO);
            msg.WriteByte((byte)p.Skills.Count);
            foreach (var item in p.Skills)
            {
                msg.WriteInt(item.TemplateId);
                msg.WriteByte(item.VariantId);
            }
            p.SendMessage(msg);
            msg.Close();
        }

        public static void SendWearingItems(Player p)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_WEARING_ITEMS_INFO);
            msg.WriteInt(p.Id);
            msg.WriteByte((byte)p.WearingItems.Length);
            for (int i = 0; i < p.WearingItems.Length; i++)
            {
                ItemEquipment item = p.WearingItems[i];
                bool has = true;
                if (item == null)
                {
                    has = false;
                }
                msg.WriteBool(has);
                if (has)
                {
                    msg.WriteString(item.Id);
                    msg.WriteInt(item.TemplateId);
                }
            }
            if (p.MapCur == null)
            {
                p.SendMessage(msg);
            }
            else
            {
                ServerManager.GI().SendAllInMap(msg, p.MapCur);
            }
            msg.Close();
        }

        public static void SendInventoryItems(Player p)
        {
            Message msg = new Message(MessageId.SERVER_PLAYER_INVENTORY_ITEMS_INFO);
            msg.WriteByte((byte)p.InventoryItems.Length);
            for (int i = 0; i < p.InventoryItems.Length; i++)
            {
                BaseItem item = p.InventoryItems[i];
                bool has = true;
                if (item == null)
                {
                    has = false;
                }
                msg.WriteBool(has);
                if (has)
                {
                    msg.WriteString(item.Id);
                    msg.WriteInt(item.TemplateId);
                    msg.WriteByte((byte)item.GetItemType());
                    if (item.GetItemType() == ItemType.Equipment)
                    {

                    }
                    else
                    {
                        int quantity = 0;
                        if (item.GetItemType() == ItemType.Consumable)
                        {
                            quantity = ((ItemConsumable)item).Quantity;
                        }
                        else
                        {
                            quantity = ((ItemMaterial)item).Quantity;
                        }
                        msg.WriteInt(quantity);
                    }
                }
            }
            p.SendMessage(msg);
            msg.Close();
        }
    }
}
