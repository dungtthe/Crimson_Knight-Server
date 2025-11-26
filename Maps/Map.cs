using Crimson_Knight_Server.Players;
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

        private List<Player> Players  = new List<Player>();

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            player.MapCur = this;
        }

    }
}
