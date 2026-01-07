using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class NpcService
{

    #region menu
    public static void ShowMenu(int npcId, Player p)
    {
        switch (npcId)
        {
            case 0:
                ShowMenuNguoiMe(npcId, p);
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
            default:
                break;
        }
    }

    #region người mẹ
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
    #endregion

    #endregion



}
