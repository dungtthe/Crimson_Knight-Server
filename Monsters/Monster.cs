using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Monsters
{
    public class Monster : BaseObject
    {
        public Monster(int id, short x, short y, MonsterTemplate template) : base(id)
        {
            this.X = x;
            this.Y = y;
            this.Template = template;
            this.Stats = template.Stats;
            this.CurrentHp = this.GetMaxHp();
        }
        public MonsterTemplate Template { get; set; }

        public override bool IsMonster()
        {
            return true;
        }

        public override bool IsPlayer()
        {
            return false;
        }

        public void SendMonsterBaseInfo(Player p)
        {
            Message msg = new Message(MessageId.SERVER_MONSTER_BASE_INFO);
            msg.WriteInt(this.Id);
            msg.WriteInt(this.CurrentHp);
            msg.WriteInt(this.GetMaxHp());
            p.SendMessage(msg);
            msg.Close();
        }
    }
}
