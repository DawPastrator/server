using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Models
{
    public record DeviceModel
    {
        public string DeviceName { get; set; }
        public string PublicKey { get; set; }
    }
}
