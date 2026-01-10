using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Monsters
{
    public class Monster : BaseObject
    {
        private Map map;
        public Monster(int id, short x, short y, MonsterTemplate template, Map map) : base(id)
        {
            this.map = map;
            this.X = x;
            this.Y = y;
            this.Template = template;
            this.Stats = template.Stats;
            this.CurrentHp = this.GetMaxHp();
        }
        public MonsterTemplate Template { get; set; }
        public long StartTimeDie { get; set; }

        public override bool IsMonster()
        {
            return true;
        }

        public override bool IsPlayer()
        {
            return false;
        }

        protected override void CheckDie()
        {
            if (this.CurrentHp <= 0)
            {
                StartTimeDie = SystemUtil.CurrentTimeMillis();
            }
        }

        public override void Update()
        {
            CheckRespawn();
        }


        void CheckRespawn()
        {
            if (IsDie())
            {
                if (SystemUtil.CurrentTimeMillis() - StartTimeDie > Template.RespawnTime)
                {
                    this.CurrentHp = this.GetMaxHp();
                    ServerMessageSender.MonsterBaseInfo(this, this.map);
                }
            }
        }
    }
}
