using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public class ItemPick
    {
        public string Id { get; set; }
        public int TemplateId { get; set; }
        public ItemType ItemType { get; set; }
        public int Quantity { get; set; }
        public long StartLeaveTime { get; set; }
        public int PlayerId { get; set; }
        

        public ItemPick(string id, int templateId, ItemType itemType, int quantity, int playerId)
        {
            this.Id = id;
            this.TemplateId = templateId;
            this.ItemType = itemType;
            this.Quantity = quantity;
            this.PlayerId = playerId;
            this.StartLeaveTime = SystemUtil.CurrentTimeMillis();
        }


        private static List<int> GetDropEquipmentIds(int monsterLevel)
        {
            int monsterDiv = monsterLevel / 10;

            if (MapManager.ItemEquipmentPickIds.TryGetValue(monsterDiv, out var list))
                return list;

            int maxTier = MapManager.ItemEquipmentPickIds.Keys.Max();
            return MapManager.ItemEquipmentPickIds[maxTier];
        }

        public static int RandomDropEquipment(int monsterLevel)
        {
            var list = GetDropEquipmentIds(monsterLevel);
            if (list == null || list.Count == 0)
                return -1;

            return list[Helpers.RanInt(0, list.Count - 1)];
        }
    }
}
