using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Monsters
{
    public class Monster : BaseObject
    {
        public Monster(int id) : base(id)
        {
        }

        public override int GetAtk()
        {
            int atk = Stats.TryGetValue(StatId.ATK, out Stats.Stat stat) ? stat.Value : 0;
            return atk;
        }

        public override int GetDef()
        {
            int def = Stats.TryGetValue(StatId.DEF, out Stats.Stat stat) ? stat.Value : 0;
            return def;
        }

        public override int GetMaxHp()
        {
            int maxHp = Stats.TryGetValue(StatId.HP, out Stats.Stat stat) ? stat.Value : 0;
            return maxHp;
        }
    }
}
