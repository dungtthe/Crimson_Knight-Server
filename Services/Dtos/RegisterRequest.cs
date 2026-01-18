using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Services.Dtos
{
    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public ClassType ClassType { get; set; }
        public Gender Gender { get; set; }
    }
}
