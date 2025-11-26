using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public class Map
    {
        public short Id;
        public string Name;

        public Map(MapTemplate template)
        {
            Id = template.Id;
            Name = template.Name;
        }

        private List<Player> Players  = new List<Player>();

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            player.MapCur = this;
        }

        public void UpdateMap()
        {
            ConsoleLogging.LogInfor("update map: " + Id);
        }
    }
}
