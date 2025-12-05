using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public interface IPlayerBroadcaster
    {
        void BroadcastEnterMap();
        void BroadcastExitMap();
        void BroadcastMove();
    }
}
