using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Stats
{
    public enum StatId:byte
    {
        HP = 0,
        MP = 1,
        ATK = 2,
        DEF = 3,

        PERCENT_HP = 4,
        PERCENT_MP = 5,
        PERCENT_ATK = 6,
        PERCENT_DEF = 7
    }
}
