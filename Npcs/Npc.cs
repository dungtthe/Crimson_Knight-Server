using Crimson_Knight_Server.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Npcs
{
    public class Npc
    {
        public short X { get; set; }
        public short Y { get; set; }
        public NpcTemplate Template { get; set; }

        public Npc(short x, short y, NpcTemplate npcTemplate)
        {
            X = x;
            Y = y;
            Template = npcTemplate;
        }
    }
}
