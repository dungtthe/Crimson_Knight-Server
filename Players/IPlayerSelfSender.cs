using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public interface IPlayerSelfSender
    {
        void SendEnterMap();
        void SendMonstersInMap();
        void SendOtherPlayersInMap();
    }
}
