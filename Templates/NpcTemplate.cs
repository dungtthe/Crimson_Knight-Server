using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Templates
{
    public class NpcTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ImageId { get; set; }
        public int ImageTalkId { get; set; }
        public string Content { get; set; }
    }
}
