using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Npcs;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class NpcService
{
    public static void ShowDialogOk(string content, Player p)
    {
        Message msg = new Message(MessageId.SERVER_SHOW_DIALOG_OK);
        msg.WriteString(content);
        p.SendMessage(msg);
        msg.Close();
    }

    public static void ShowDialogYesNo(string content,DialogYesNoId dialogYesNoId, Player p)
    {
        Message msg = new Message(MessageId.SERVER_SHOW_DIALOG_YES_NO);
        msg.WriteString(content);
        msg.WriteByte((byte)dialogYesNoId);
        p.SendMessage(msg);
        msg.Close();
    }


    public static void ShowMenu(int npcId, Player p)
    {
        switch (npcId)
        {
            case 0:
                ShowMenuNguoiMe(npcId, p);
                break;
            case 1:
                ShowMenuKiemSi(npcId, p);
                break;
            default:
                break;
        }
    }

 

    public static void SelectMenuItem(int npcId, byte menuItemId, Player p)
    {
        switch (npcId)
        {
            case 0:
                SelectMenuItemNguoiMe(npcId, menuItemId, p);
                break;
            case 1:
                SelectMenuItemKiemSi(menuItemId, p);
                break;
            default:
                break;
        }
    }

  
    private static void ShowMenuNguoiMe(int npcId, Player p)
    {
        Message msg = new Message(MessageId.SERVER_SHOW_MENU);
        msg.WriteInt(npcId);
        msg.WriteString("E hehe he");
        msg.WriteByte(3);
        msg.WriteString("Xin chào " + 1);
        msg.WriteString("Xin chào " + 2);
        msg.WriteString("Xin chào " + 3);
        p.SendMessage(msg);
        msg.Close();
    }

    private static void SelectMenuItemNguoiMe(int npcId, byte menuItemId, Player p)
    {
        ConsoleLogging.LogInfor($"Player {p.Name} selected menu item {menuItemId} from NPC {npcId}");
    }

    private static void ShowMenuKiemSi(int npcId, Player p)
    {
        Message msg = new Message(MessageId.SERVER_SHOW_MENU);
        msg.WriteInt(npcId);
        msg.WriteString("Menu");
        msg.WriteByte(2);
        msg.WriteString("Vào phó bản!");
        msg.WriteString("Tính năng khác");
        p.SendMessage(msg);
        msg.Close();
    }

    private static void SelectMenuItemKiemSi(byte menuItemId, Player p)
    {
        switch (menuItemId)
        {
            case 0:
                ShowDialogYesNo("Bạn có muốn vào phó bản không?", DialogYesNoId.ENTER_PHO_BAN, p);
                break;
            default:
                ShowDialogOk("Chưa có tính năng này!", p);
                break;
        }
    }



    public static void HandleDialogYesNo(DialogYesNoId id, bool isOk, Player p)
    {
        if (isOk)
        {
            switch (id)
            {
                case DialogYesNoId.ENTER_PHO_BAN:

                break;
            }
        }
    }

}
