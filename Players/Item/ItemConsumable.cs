using Crimson_Knight_Server.Templates.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players.Item
{
    public class ItemConsumable : BaseItem
    {
        public ItemConsumable(int templateId, int quantity)
        {
            this.TemplateId = templateId;
            this.Id = templateId.ToString();
            this.Quantity = quantity;
        }
        public int Quantity { get; set; }
        public override ItemType GetItemType()
        {
            return ItemType.Consumable;
        }

        public static ItemConsumable Create(JsonElement data)
        {
            return new ItemConsumable(
                data[0].GetInt32(),
                data[1].GetInt32()
            );
        }

        public object[] ToSaveData()
        {
            return new object[]
            {
                TemplateId,
                Quantity
            };
        }

    }
}
