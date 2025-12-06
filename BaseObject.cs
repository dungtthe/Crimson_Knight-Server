using Crimson_Knight_Server.Stats;
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

        public Dictionary<StatId, Stat> Stats = new Dictionary<StatId, Stat>();
        public abstract int GetMaxHp();
        public abstract int GetAtk();
        public abstract int GetDef();
    }
}
