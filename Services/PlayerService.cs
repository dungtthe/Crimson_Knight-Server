using Crimson_Knight_Server.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Services
{
    public class PlayerService
    {
        private static PlayerService ins;
        public static PlayerService GI()
        {
            if (ins == null)
            {
                ins = new PlayerService();
            }
            return ins;
        }



    }
}
