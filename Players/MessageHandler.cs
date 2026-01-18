using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Maps.MessageMap.Attack;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Npcs;
using Crimson_Knight_Server.Players.Item;
using Crimson_Knight_Server.Services;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Templates.Shops;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class MessageHandler
    {
        private Player session;
        public MessageHandler(Player session)
        {
            this.session = session;
        }
        public void HandleMessage(Message msg)
        {
            if (msg.Id == MessageId.CLIENT_LOGIN)
            {
                string token = msg.ReadString();
                string[] s = token.Split('.');
                int id = int.Parse(s[0]);

                //check lai
                if (!PlayerService.SetupPlayer(session, id))
                {
                    session.Close();
                }
                msg.Close();
                ServerMessageSender.Login(session);
                session.FinalSetup();
                return;
            }
            if (session.Id == -1)
            {
                ConsoleLogging.LogWarning("Chưa đăng nhập mà gửi message khác LOGIN");
                msg.Close();
                return;
            }
            switch (msg.Id)
            {
                case MessageId.CLIENT_PLAYER_MOVE:
                    {
                        int x = msg.ReadInt();
                        int y = msg.ReadInt();
                        session.X = (short)x;
                        session.Y = (short)y;
                        ConsoleLogging.LogInfor($"[Player {session.Id}] Move to ({x},{y})");
                        msg.Close();
                        ServerMessageSender.PlayerMove(this.session, (short)x, (short)y);
                        break;
                    }
                case MessageId.CLIENT_ENTER_MAP:
                    {
                        //short departId = msg.ReadShort();
                        //if (session.MapCur != null)
                        //{
                        //    var mapCur = MapManager.Maps[session.MapCur.Id];
                        //    mapCur.BusPlayerExitMap.Enqueue(session);
                        //}

                        //var depart = MapManager.DepartTemplates[departId];
                        //var mapEnter = MapManager.Maps[depart.MapEnterId];
                        //session.X = depart.XEnter;
                        //session.Y = depart.YEnter;
                        //mapEnter.BusPlayerEnterMap.Enqueue(session);
                        //msg.Close();


                        short departId = msg.ReadShort();
                        msg.Close();
                        var depart = MapManager.DepartTemplates[departId];
                        var mapEnter = MapManager.Maps[depart.MapEnterId];
                        //session.X = depart.XEnter;
                        //session.Y = depart.YEnter;
                        MapManager.PlayerEnterOrExitmap.Enqueue(new Tuple<Map, Player, bool, short, short>(mapEnter, session, true, depart.XEnter, depart.YEnter));
                        break;
                    }
                case MessageId.CLIENT_SHOW_MENU:
                    {
                        int npcId = msg.ReadInt();
                        msg.Close();
                        NpcService.ShowMenu(npcId, session);
                        break;
                    }
                case MessageId.CLIENT_SELECT_MENU_ITEM:
                    {
                        int npcId = msg.ReadInt();
                        byte menuItemId = msg.ReadByte();
                        msg.Close();
                        NpcService.SelectMenuItem(npcId, menuItemId, session);
                        break;
                    }
                case MessageId.CLIENT_PLAYER_ATTACK:
                    {
                        int skillUsedId = msg.ReadInt();
                        byte size = msg.ReadByte();
                        bool[] isPlayers = new bool[size];
                        int[] targetIds = new int[size];
                        for (int i = 0; i < size; i++)
                        {
                            isPlayers[i] = msg.ReadBool();
                        }
                        for (int i = 0; i < size; i++)
                        {
                            targetIds[i] = msg.ReadInt();
                        }
                        msg.Close();
                        if (isPlayers.Length == 0 || isPlayers.Length != targetIds.Length)
                        {
                            return;
                        }
                        session.MapCur?.AttackMessages.Add(new AttackMessage()
                        {
                            PlayerSenderId = session.Id,
                            SkillUseId = skillUsedId,
                            IsPlayers = isPlayers,
                            TargetIds = targetIds
                        });
                        break;
                    }
                case MessageId.CLIENT_PLAYER_CHANGE_PKTYPE:
                    byte pktype = msg.ReadByte();
                    msg.Close();
                    if (pktype > (byte)PkType.Yellow)
                    {
                        return;
                    }
                    PkType pkType = (PkType)pktype;
                    session.PkType = pkType;
                    ServerMessageSender.SendPkType(session);
                    break;
                case MessageId.CLIENT_PICK_ITEM:
                    string id = msg.ReadString();
                    msg.Close();
                    if(session.MapCur == null)
                    {
                        return;
                    }
                    session.MapCur.PickItemMessages.Add(new Maps.MessageMap.PickItemMsg(id, session));
                    break;
                case MessageId.CLIENT_USE_ITEM:
                    string idItem = msg.ReadString();
                    ItemType type = (ItemType)msg.ReadByte();
                    session.UseItemMsgs.Enqueue(new MessagePlayer.UseItemMsg(idItem,type));
                    break;
                case MessageId.CLIENT_SELECT_DIALOG_YES_NO:
                    DialogYesNoId dialogYesNoId = (DialogYesNoId)msg.ReadByte();
                    bool isOk = msg.ReadBool();
                    NpcService.HandleDialogYesNo(dialogYesNoId, isOk, session);
                    break;
                case MessageId.CLIENT_BUY_ITEM:
                    int templateId = msg.ReadInt();
                    type = (ItemType)msg.ReadByte();
                    int quantity = msg.ReadInt();
                    if(quantity<0 || quantity > 1000)
                    {
                        ServerMessageSender.CenterNotificationView(session, "Số lượng phải >0 và <= 1000");
                        return;
                    }
                    ItemShop itemshop = ItemShop.GetItem(templateId, type);
                    if (itemshop == null)
                    {
                        ServerMessageSender.CenterNotificationView(session, "Không tìm thấy vật phẩm");
                        return;
                    }
                    session.BuyItems.Enqueue(new Tuple<ItemShop, int>(itemshop, quantity));
                    break;
                default:
                        break;
            }
        }


    }
}
