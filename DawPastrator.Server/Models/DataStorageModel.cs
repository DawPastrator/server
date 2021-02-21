﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Models
{
    public record DataStorageModel
    {
        public DateTime Time { get; set; }
        public string Bs4Data { get; set; } = "";
    }
}
