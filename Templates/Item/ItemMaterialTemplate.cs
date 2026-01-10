using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates.Item
{
    public class ItemMaterialTemplate : ItemTemplateBase
    {
        public override ItemType GetItemType()
        {
            return ItemType.Material;
        }
    }
}
