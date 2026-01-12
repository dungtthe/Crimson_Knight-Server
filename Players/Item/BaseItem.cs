using Crimson_Knight_Server.Templates;
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

        public string GetName()
        {
            if (GetItemType() == ItemType.Equipment)
            {
                return TemplateManager.ItemEquipmentTemplates[TemplateId].Name;
            }

            if (GetItemType() == ItemType.Consumable)
            {
                return TemplateManager.ItemConsumableTemplates[TemplateId].Name;
            }

            return TemplateManager.ItemMaterialTemplates[TemplateId].Name;
        }

    }
}
