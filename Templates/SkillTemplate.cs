using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public class SkillTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int LevelPlayerRequire { get; set; }
        public bool IsBuff { get; set; }
        public short IconId { get; set; }

        public List<Variant> Variants { get; set; }
        public class Variant
        {
            public short MpLost { get; set; }
            public int Cooldown { get; set; }
            public short Range { get; set; }
            public byte TargetCount { get; set; }
            public Dictionary<StatId, Stat> Stats { get; set; }
        }
    }
}
