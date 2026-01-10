using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates.Item
{
    public class ItemEquipmentTemplate : ItemTemplateBase
    {
        public Dictionary<StatId, Stat> Stats { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ClassType ClassType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EquipmentType EquipmentType { get; set; }
        public int PartId { get; set; }

        public override ItemType GetItemType()
        {
            return ItemType.Equipment;
        }
    }
}
