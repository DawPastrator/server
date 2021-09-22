using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace DawPastrator.Server.Models
{
    public class DevicePublicKeyModel
    {
        // 设备uuid
        [Key]
        public string DeviceId { get; set; }

        // 公钥
        public string PublicKey { get; set; }

        // 所属用户，外键
        public int UserId { get; }

        [ForeignKey("UserId")]
        public UserInfoModel UserInfo { get; }
    }
}