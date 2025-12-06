using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.DataAccessLayer.Models
{
    public class PlayerModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public short MapId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public string Stats;
        public byte ClassType;
    }
}
