using Crimson_Knight_Server.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps.MessageMap
{
    public class PickItemMsg
    {
        public string Id { get; set; }
        public Player Player { get; set; }

        public PickItemMsg(string id, Player player)
        {
            Id = id; 
            Player = player;
        }
    }
}
