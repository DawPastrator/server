using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DawPastrator.Server.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<UserInfoModel> userInfos;
        public DbSet<DevicePublicKeyModel> devicesAndPublicKeys;

        public string DbPath { get; private set; }

        public DatabaseContext()
        {
            var currentPath = Environment.CurrentDirectory;
            DbPath = $"{currentPath}{System.IO.Path.DirectorySeparatorChar}UserData.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        // 配置键属性
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
