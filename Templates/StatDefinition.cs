using Crimson_Knight_Server.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public class StatDefinition
    {
        public StatId StatId { get; set; }
        public string Name { get; init; }
        public string Description { get; init; }
        public bool IsPercent { get; init; }
    }
}
