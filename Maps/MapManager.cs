using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public static class MapManager
    {
        public static readonly List<Map> Maps = new List<Map>();
        public static List<DepartTemplate> DepartTemplates;



        public static readonly ConcurrentQueue<Tuple<Map, Player, bool,short,short>> PlayerEnterOrExitmap = new();//true la enter
    }
}
