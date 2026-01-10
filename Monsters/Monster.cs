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
        private long startTimeDie;
        private long startTimeAttack;

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
                startTimeDie = SystemUtil.CurrentTimeMillis();
            }
        }

        public override void Update()
        {
            CheckRespawn();
            AttackPlayer();
        }


        void CheckRespawn()
        {
            if (IsDie())
            {
                if (SystemUtil.CurrentTimeMillis() - startTimeDie > Template.RespawnTime)
                {
                    this.CurrentHp = this.GetMaxHp();
                    ServerMessageSender.MonsterBaseInfo(this, this.map);
                }
            }
        }

        void AttackPlayer()
        {
            if (IsDie())
            {
                return;
            }
            if(SystemUtil.CurrentTimeMillis() - startTimeAttack < Template.Cooldown)
            {
                return;
            }
            int range = Template.Range;
            int targetCount = Template.TargetCount;
            List<Player> targets = new List<Player>();
            foreach (var p in map.Players)
            {
                if (p.IsDie())
                {
                    continue;
                }
                int dis = MathUtil.Distance(this, p);
                if (dis > range)
                {
                    continue;
                }

                targets.Add(p);

                if(targets.Count >= targetCount)
                {
                    break;
                }
            }
            if(targets.Count == 0)
            {
                return;
            }
            this.startTimeAttack = SystemUtil.CurrentTimeMillis();

            foreach(var p in targets)
            {
                int dam = this.GetAtk() - p.GetDef();
                if(dam <= 0)
                {
                    dam = 1;
                }
                p.CurrentHp -= dam;
                ServerMessageSender.PlayerBaseInfo(p, true);
                SendAttackInfoMsg(dam,targets);
            }
        }



        void SendAttackInfoMsg(int dam, List<Player>targets)
        {
            Message msg = new Message(MessageId.SERVER_MONSTER_ATTACK);
            msg.WriteInt(this.Id);
            msg.WriteInt(dam);
            msg.WriteByte((byte)targets.Count);
            foreach(var p in targets)
            {
                msg.WriteInt(p.Id);
            }
            ServerManager.GI().SendAllInMap(msg, this.map);
            msg.Close();
        }
    }
}
