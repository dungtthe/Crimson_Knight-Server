using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates.Shops
{
    public class ItemShop
    {
        public int IdItem { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ItemType ItemType { get; set; }
        public int Price { get; set; }


        public static ItemShop GetItem(int id,  ItemType itemType)
        {
            foreach(var item in TemplateManager.ItemShops)
            {
                if(item.IdItem == id &&  item.ItemType == itemType)
                {
                    return item;
                }
            }
            return null;
        }

        public string GetName()
        {
            if (ItemType == ItemType.Equipment)
            {
                return TemplateManager.ItemEquipmentTemplates[IdItem].Name;
            }

            if (ItemType == ItemType.Consumable)
            {
                return TemplateManager.ItemConsumableTemplates[IdItem].Name;
            }

            return TemplateManager.ItemMaterialTemplates[IdItem].Name;
        }
    }
}
