using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DawPastrator.Server.Models
{
    public class DataStorageModel : DbContext
    {
        public string Bs4Data { get; set; } = "";
    }



}
