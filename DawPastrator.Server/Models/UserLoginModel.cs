using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Models
{
    public record UserLoginModel
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
