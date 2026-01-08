using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Utils
{
    public static class Helpers
    {
        public static float GetPercent(int value)
        {
            return value / 10000f;
        }
    }
}
