using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps.MessageMap.Attack
{
    public class AttackMessage
    {
        public int PlayerSenderId { get; set; }
        public int SkillUseId { get; set; }
        public bool[] IsPlayers { get; set; }
        public int[] TargetIds { get; set; }
    }
}
