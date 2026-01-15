using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public class QuestTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MonsterTemplateId { get; set; }
        public int Quantity { get; set; }
        public int NpcId { get; set; }
        public int GoldReceive { get; set; }
        public int EXPReceive { get; set; }
    }
}
