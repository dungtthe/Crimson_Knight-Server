using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates.Item
{
    public abstract class ItemTemplateBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short LevelRequire { get; set; }
        public int IconId { get; set; }
        public abstract ItemType GetItemType();
    }
}
