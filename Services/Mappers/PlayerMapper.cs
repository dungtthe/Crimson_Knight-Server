using Crimson_Knight_Server.DataAccessLayer.Models;
using Crimson_Knight_Server.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Services.Mappers
{
    public static class PlayerMapper
    {
        public static Player MapToPlayer(Player player, PlayerModel model)
        {
            player.PlayerId = model.Id;
            player.Name = model.Name;
            return player;
        }
    }
}
