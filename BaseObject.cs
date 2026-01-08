using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server
{
    public abstract class BaseObject
    {

        protected BaseObject(int id)
        {
            Id = id;
        }
        public int Id { get; private set; }
        public string Name { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public Dictionary<StatId, Stat> Stats = new Dictionary<StatId, Stat>();
        protected int GetStatValue(StatId id)
        {
            return Stats.TryGetValue(id, out Stat stat) ? stat.Value : 0;
        }

        public virtual int GetMaxHp()
        {
            int hp = GetStatValue(StatId.HP);
            int percentHp = GetStatValue(StatId.PERCENT_HP);

            return hp + (int)((long)hp * percentHp / 10000);
        }

        public virtual int GetAtk()
        {
            int atk = GetStatValue(StatId.ATK);
            int percentAtk = GetStatValue(StatId.PERCENT_ATK);

            return atk + (int)((long)atk * percentAtk / 10000);
        }
        public virtual int GetDef()
        {
            int def = GetStatValue(StatId.DEF);
            int percentDef = GetStatValue(StatId.PERCENT_DEF);

            return def + (int)((long)def * percentDef / 10000);
        }

        public virtual int GetMaxMp()
        {
            int mp = GetStatValue(StatId.MP);
            int percentMp = GetStatValue(StatId.PERCENT_MP);

            return mp + (int)((long)mp * percentMp / 10000);
        }
    }
}
