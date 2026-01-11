using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players.Item
{
    public abstract class BaseItem
    {
        public string Id { get; set; }
        public int TemplateId { get; set; }
        public abstract ItemType GetItemType();
    }
}
