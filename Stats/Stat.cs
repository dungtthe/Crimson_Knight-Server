using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Stats
{
    public class Stat
    {
        public StatId Id { get; set; }
        public int Value { get; set; }
        public Stat() { }
    }
}
