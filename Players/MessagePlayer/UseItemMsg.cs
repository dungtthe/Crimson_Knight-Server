using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players.MessagePlayer
{
    public class UseItemMsg
    {
        public UseItemMsg(string idItem, ItemType type)
        {
            this.ItemId = idItem;
            this.ItemType = type;
        }
        public string ItemId { get; set; }
        public ItemType ItemType { get; set; }
    }
}
