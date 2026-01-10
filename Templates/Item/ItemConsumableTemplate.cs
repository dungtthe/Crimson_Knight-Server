using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates.Item
{
    public class ItemConsumableTemplate : ItemTemplateBase
    {
        public int Cooldown { get; set; }
        public long Value { get; set; }
        public override ItemType GetItemType()
        {
            return ItemType.Consumable;
        }
    }
}
