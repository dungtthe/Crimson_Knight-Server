using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Networking
{
    public enum MessageId:short
    {
        LOGIN,
        PLAYER_MOVE,
        OTHER_PLAYER_MOVE
    }
}
