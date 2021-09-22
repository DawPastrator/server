using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DawPastrator.Server.Models
{
    [Index(nameof(UserName), IsUnique = true)] // 对用户名做索引
    public class UserInfoModel
    {
        // 用户ID，自增主键
        [Key]
        public int UserId { get; }

        // 用户名，非空，varchar(15)，索引
        [Required]
        [Column(TypeName = "varchar(15)")]
        public string UserName { get; set; }

        // 用户主密码加密后的base64字符，非空
        [Required]
        public string EncryptedMasterPassword { get; set; }

        // 加密后的数据base64，理论上不止可以做密码管理器，这里可以装任何想要保密的数据
        // 该字段为medium text最大存储16M数据
        [Required]
        [Column(TypeName = "mediumtext")]
        public string PasswordData { get; set; }

        // 设备和公钥
        public List<DevicePublicKeyModel> DevicesAndPublicKeys { get; set; }
    }
}
