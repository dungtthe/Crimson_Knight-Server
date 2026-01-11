using Crimson_Knight_Server.Templates.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players.Item
{
    public class ItemMaterial : BaseItem
    {
        public ItemMaterial(int templateId, int quantity)
        {
            this.TemplateId = templateId;
            this.Id = templateId.ToString();
            this.Quantity = quantity;
        }
        public int Quantity { get; set; }

        public override ItemType GetItemType()
        {
            return ItemType.Material;
        }

        public static ItemMaterial Create(JsonElement data)
        {
            return new ItemMaterial(
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
