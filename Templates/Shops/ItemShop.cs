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
    }
}
