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
        public int Id { get; protected set; }
        public string Name { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }
        public short Level { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public Dictionary<StatId, Stat> Stats = new Dictionary<StatId, Stat>();

        public virtual int GetMaxHp()
        {
            int hp = Helpers.GetStatValue(this.Stats, StatId.HP);
            int percentHp = Helpers.GetStatValue(this.Stats, StatId.PERCENT_HP);

            return hp + (int)((long)hp * percentHp / 10000);
        }

        public virtual int GetAtk()
        {
            int atk = Helpers.GetStatValue(this.Stats, StatId.ATK);
            int percentAtk = Helpers.GetStatValue(this.Stats, StatId.PERCENT_ATK);

            return atk + (int)((long)atk * percentAtk / 10000);
        }
        public virtual int GetDef()
        {
            int def = Helpers.GetStatValue(this.Stats, StatId.DEF);
            int percentDef = Helpers.GetStatValue(this.Stats, StatId.PERCENT_DEF);

            return def + (int)((long)def * percentDef / 10000);
        }

        public virtual int GetMaxMp()
        {
            int mp = Helpers.GetStatValue(this.Stats, StatId.MP);
            int percentMp = Helpers.GetStatValue(this.Stats, StatId.PERCENT_MP);

            return mp + (int)((long)mp * percentMp / 10000);
        }
    }
}
