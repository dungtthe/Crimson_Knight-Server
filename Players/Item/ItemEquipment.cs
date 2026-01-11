using Crimson_Knight_Server.Templates.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crimson_Knight_Server.Players.Item
{
    public class ItemEquipment : BaseItem
    {
        public ItemEquipment(string id, int templateId)
        {
            this.Id = id;
            this.TemplateId = templateId;
        }
        public override ItemType GetItemType()
        {
            return ItemType.Equipment;
        }

        public static ItemEquipment Create(JsonElement data)
        {
            return new ItemEquipment(
                data[0].GetString(),
                data[1].GetInt32()
            );
        }

        public object[] ToSaveData()
        {
            return new object[]
            {
                Id,
                TemplateId
            };
        }
    }
}
