using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public class Map
    {
        public readonly ConcurrentQueue<Player> BusPlayerEnterMap = new ConcurrentQueue<Player>();
        public readonly ConcurrentQueue<Player> BusPlayerExitMap = new ConcurrentQueue<Player>();


        public short Id => Template.Id;
        public MapTemplate Template;

        public Map(MapTemplate template)
        {
            this.Template = template;   
        }

        public List<Player> Players  = new List<Player>();

        private void PlayerEnterMap(Player player)
        {
            Players.Add(player);
            player.MapCur = this;
            player.SendEnterMap();
            player.BroadcastEnterMap();
        }

        public void UpdateMap()
        {
            while (BusPlayerEnterMap.TryDequeue(out Player playerEnter))
            {
                PlayerEnterMap(playerEnter);
                ConsoleLogging.LogInfor($"Player {playerEnter.PlayerId} đã vào map {Id}");
            }
        }
    }
}
