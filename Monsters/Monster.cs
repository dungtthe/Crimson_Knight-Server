using Crimson_Knight_Server.Maps;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public bool IsBoss { get; set; }

        public override bool IsMonster()
        {
            return true;
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
            MoveToPlayer();
        }


        private long startTimeMove = 0;
        private void MoveToPlayer()
        {
            if (!IsBoss)
            {
                return;
            }
            if (map.Players.Count == 0)
            {
                return;
            }
            if (SystemUtil.CurrentTimeMillis() - startTimeMove > 10000)
            {
                startTimeMove = SystemUtil.CurrentTimeMillis();
                int index = Helpers.RanInt(0, map.Players.Count - 1);
                Player p = map.Players[index];
                this.X = p.X;
                this.Y = p.Y;
                ServerMessageSender.MonsterMove(this.Id, p.X, p.Y, p.Id, map);
            }
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
            if (SystemUtil.CurrentTimeMillis() - startTimeAttack < Template.Cooldown)
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

                if (targets.Count >= targetCount)
                {
                    break;
                }
            }
            if (targets.Count == 0)
            {
                return;
            }
            this.startTimeAttack = SystemUtil.CurrentTimeMillis();

            foreach (var p in targets)
            {
                int dam = this.GetAtk() - p.GetDef();
                if (dam <= 0)
                {
                    dam = 1;
                }
                p.CurrentHp -= dam;
                ServerMessageSender.PlayerBaseInfo(p, true);
                SendAttackInfoMsg(dam, targets);
            }
        }



        void SendAttackInfoMsg(int dam, List<Player> targets)
        {
            Message msg = new Message(MessageId.SERVER_MONSTER_ATTACK);
            msg.WriteInt(this.Id);
            msg.WriteInt(dam);
            msg.WriteByte((byte)targets.Count);
            foreach (var p in targets)
            {
                msg.WriteInt(p.Id);
            }
            ServerManager.GI().SendAllInMap(msg, this.map);
            msg.Close();
        }

        public override void TakeDamage(int dam, BaseObject attacker)
        {
            int hpPre = CurrentHp;
            base.TakeDamage(dam, attacker);

            int exp = (int)((float)(Math.Abs(CurrentHp - hpPre)) / 5);

            if (attacker != null)
            {
                if (attacker.IsPlayer())
                {
                    ((Player)attacker).UpdateExp(exp);
                }

                if (IsDie())
                {
                    AfterDie(attacker);
                }
            }
        }

        protected override void AfterDie(BaseObject attacker)
        {
            if (attacker != null)
            {
                Player p = attacker as Player;
                if (p.Quest != null && p.Quest.QuestState == QuestState.InProgress)
                {
                    if (p.Quest.GetTemplate().MonsterTemplateId == this.Template.Id)
                    {
                        p.Quest.QuantityCur++;
                        string content = $"Nhiệm vụ {p.Quest.GetTemplate().Name}: {p.Quest.QuantityCur}/{p.Quest.GetTemplate().Quantity}";
                        ServerMessageSender.CenterNotificationView(p, content);
                        if (p.Quest.QuantityCur >= p.Quest.GetTemplate().Quantity)
                        {
                            p.Quest.QuestState = QuestState.Completed;
                            ServerMessageSender.CenterNotificationView(p, $"Hoàn thành nhiệm vụ {p.Quest.GetTemplate().Name}");
                            ServerMessageSender.SendQuest(p);
                        }
                    }
                }
            }

            //hp,mp
            if (Helpers.Roll(6000))
            {
                //hp
                int idConsu = 0;
                if (this.Template.Level >= 10)
                {
                    idConsu = 2;
                }
                //mp
                if (Helpers.Roll(5000))
                {
                    idConsu += 1;
                }
                this.map.DropItem(idConsu, ItemType.Consumable, attacker.Id, this);
                return;
            }
            //material
            if (Helpers.Roll(5000))
            {
                //tam thoi nhu nay
                this.map.DropItem(0, ItemType.Material, attacker.Id, this);
                return;
            }

            //vang
            if (Helpers.Roll(4000))
            {
                int vangRan = this.Template.Level * Helpers.RanInt(10, 40);
                this.map.DropItem(-1, ItemType.Consumable, attacker.Id, this, vangRan);
                return;
            }


            if (Helpers.Roll(2000))
            {
                int idEqLeave = ItemPick.RandomDropEquipment(this.Template.Level);
                if (idEqLeave == -1)
                {
                    return;
                }

                this.map.DropItem(idEqLeave, ItemType.Equipment, attacker.Id, this);
            }
        }
    }
}
