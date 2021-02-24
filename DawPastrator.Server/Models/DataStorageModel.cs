using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Models
{
    public record DataStorageModel
    {
        public DeviceModel Device { get; set; } = new DeviceModel();
        public string Bs4Data { get; set; } = "";
    }
}