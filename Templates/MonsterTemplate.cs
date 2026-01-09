using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public class MonsterTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ImageId { get; set; }
        public short Level { get; set; }
        public int Cooldown { get; set; }
        public Dictionary<StatId, Stat> Stats { get; set; }
    }
}
