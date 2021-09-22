using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Google.Protobuf;
using DawPastrator.Core;
using DawPastrator.Services.DevicesAndPublicKeysInfo;
using DawPastrator.Server.Models;

namespace DawPastrator.Server.Models
{
    public enum DatabaseError
    {
        SUCCESS,
        ERROR_USERID_NOT_EXISTS,
        ERROR_USERNAME_NOT_EXISTS,
        ERROR_USERNAME_HAS_ALREADY_EXISTS,
        ERROR_USERNAME_IS_NULL_OR_WHITESPACE,
        ERROR_MASTERPASSWORD_IS_NULL_OR_WHITESPACE,
        ERROR_FAIL_TO_CREATE_TABLE,
        ERROR_FAIL_TO_INSERT_ROW
    }

    /// <summary>
    /// 实现数据库的CRUD
    /// </summary>
    public interface IDatabaseServicesOld : IDisposable
    {

        Task<DatabaseError> CreateAccountAsync(string userName, string masterPassword);

        Task<int> GetUserIDAsync(string userName);

        Task<List<(string, string)>> GetDevicesAndPublicKeysInfoAsync(int userID);

        Task<bool> VerifyMasterPasswordAsync(string userName, string masterPassword);

        Task<DatabaseError> UpdatePasswordsDataAsync(int userID, byte[] passwordsData);

        Task<DatabaseError> UpdateDevicesAndPublicKeysInfoAsync(int userID, List<(string, string)> devicesAndPublicKeysInfo);

        Task<DatabaseError> DeleteAccountAsync(int userID);

        Task<byte[]> GetPasswordsData(int userID);
    }

    public class SqliteDatabaseServices : IDatabaseServicesOld
    {
        private DatabaseContext db;

        public SqliteDatabaseServices()
        {
            db = new();
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }

        //private bool TableHasBeenCreated(string tableName)
        //{
        //}

        /// <summary>
        /// 创建表格
        /// </summary>
        /// <returns>状态码</returns>
        //private DatabaseError CreateTable()
        //{
        //    if (!TableHasBeenCreated("user_data"))
        //    {
        //        using SqliteCommand command = connection_.CreateCommand();

        //        command.CommandText =
        //        @"
        //        CREATE TABLE user_data (
        //            userID INTEGER PRIMARY KEY AUTOINCREMENT,
        //            userName varchar[40] NOT NULL,
        //            masterPassword CHAR[44] NOT NULL,
        //            passwordsData BLOB,
        //            devicesAndPublicKeysInfo BLOB
        //        )
        //        ";
        //        command.ExecuteNonQuery();

        //        // 前缀索引
        //        command.CommandText = "CREATE INDEX userNameIndex ON user_data (substr(userName, 0, 4));";
        //        command.ExecuteNonQuery();

        //        if (!TableHasBeenCreated("user_data"))
        //            return DatabaseError.ERROR_FAIL_TO_CREATE_TABLE;
        //    }
        //    return DatabaseError.SUCCESS;
        //}

        private bool UserNameHasAlreadyExists(string userName)
        {
            using var command = connection_.CreateCommand();

            command.CommandText = "SELECT userName FROM user_data WHERE userName = $userName";
            command.Parameters.AddWithValue("$userName", userName);

            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        /// <summary>
        /// 创建账户
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        /// <returns>状态码</returns>
        public async Task<DatabaseError> CreateAccountAsync(string userName, string masterPassword)
        {
            using var command = connection_.CreateCommand();

            if (string.IsNullOrWhiteSpace(userName))
                return DatabaseError.ERROR_USERNAME_IS_NULL_OR_WHITESPACE;

            if (string.IsNullOrWhiteSpace(masterPassword))
                return DatabaseError.ERROR_MASTERPASSWORD_IS_NULL_OR_WHITESPACE;

            if (UserNameHasAlreadyExists(userName))
                return DatabaseError.ERROR_USERNAME_NOT_EXISTS;

            // user_data
            command.CommandText =
                @"INSERT INTO user_data (userID, userName, masterPassword, passwordsData ,devicesAndPublicKeysInfo)
                    VALUES(null, $userName, $masterPassword, null, null)";

            command.Parameters.AddWithValue("$userName", userName);
            command.Parameters.AddWithValue("$masterPassword", masterPassword.AddSaltAndEncrypt(userName));

            if (await command.ExecuteNonQueryAsync() != 1)
                return DatabaseError.ERROR_FAIL_TO_INSERT_ROW;

            return DatabaseError.SUCCESS;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>用户ID</returns>
        public async Task<int> GetUserIDAsync(string userName)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT userID FROM user_data WHERE userName = $userName";
            command.Parameters.AddWithValue("$userName", userName);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
                return reader.GetInt32(0);
            else
                throw new Exception("用户名不存在");
        }

        private async Task<string> GetUserName(int userID)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT userName FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
                return reader.GetString(0);
            else
                throw new Exception("用户名不存在");
        }

        /// <summary>
        /// 获取主密码
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>主密码</returns>
        private async Task<string> GetMasterPassword(int userID)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT masterPassword FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
                return reader.GetString(0);
            else
                throw new Exception("用户ID不存在");
        }

        public async Task<bool> VerifyMasterPasswordAsync(string userName, string masterPassword)
        {
            var userID = await GetUserIDAsync(userName);
            var masterPasswordFromDatabase = await GetMasterPassword(userID);
            var processedMasterPassword = masterPassword.AddSaltAndEncrypt(userName);

            return masterPasswordFromDatabase == processedMasterPassword;
        }

        /// <summary>
        /// 获取二进制数据
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="requiredField">要获取的字段名</param>
        /// <returns>二进制数据</returns>
        private async Task<byte[]> GetBlobData(int userID, string requiredField)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT " + requiredField + " FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);
            //command.Parameters.AddWithValue("$requiredfield", requiredfield);

            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
                throw new Exception("用户ID不存在");

            // 预设的最大读取字节数，它的值应该来自config
            const int maxBytesOfPasswordsData = 10240;

            byte[] passwordsData = new byte[maxBytesOfPasswordsData];

            // 实际上读取的字节数
            long actualBytesLength = reader.GetBytes(0, 0, passwordsData, 0, maxBytesOfPasswordsData);

            // 新建长度恰好的字节数组并复制内容
            byte[] actualBytes = new byte[actualBytesLength];
            Array.Copy(passwordsData, actualBytes, actualBytesLength);

            return actualBytes;
        }

        /// <summary>
        /// 获取密码数据
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>二进制的密码数据</returns>
        public Task<byte[]> GetPasswordsData(int userID)
        {
            return GetBlobData(userID, "passwordsData");
        }

        /// <summary>
        /// 获取设备信息与公私钥信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>关于设备信息的二进制数据</returns>
        public async Task<List<(string, string)>> GetDevicesAndPublicKeysInfoAsync(int userID)
        {
            var bytesData = await GetBlobData(userID, "devicesAndPublicKeysInfo");

            var output = new List<(string, string)>();

            var devicesAndPublicKeys = DevicesAndPublicKeys.Parser.ParseFrom(bytesData);

            foreach (var deviceAndPublicKey in devicesAndPublicKeys.DeviceAndPublicKey)
            {
                output.Add((deviceAndPublicKey.DeviceID, deviceAndPublicKey.PublicKey));
            }

            return output;
        }

        /// <summary>
        /// 更新user_data中的字符串字段
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldValue">字段值</param>
        private async Task<DatabaseError> UpdateStringfield(int userID, string fieldName, string fieldValue)
        {
            // 这里可以加上判断，判断用户ID是否存在、是否是唯一的。

            using var command = connection_.CreateCommand();
            command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
            //command.Parameters.AddWithValue("$fieldName", fieldName);
            command.Parameters.AddWithValue("$fieldValue", fieldValue);
            command.Parameters.AddWithValue("$userID", userID);

            if (await command.ExecuteNonQueryAsync() != 1)
                return DatabaseError.ERROR_USERID_NOT_EXISTS;

            return DatabaseError.SUCCESS;
        }

        public async Task<DatabaseError> UpdateMasterPassword(int userID, string masterPassword)
        {
            var userName = await GetUserName(userID);
            // 以userID的值作为pbe加密的盐进行加密
            return await UpdateStringfield(userID, "masterPassword", masterPassword.AddSaltAndEncrypt(userName));
        }

        private async Task<DatabaseError> UpdateBytesfield(int userID, string fieldName, byte[] fieldValue)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
            //command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID LIMIT 1";
            //command.Parameters.AddWithValue("$fieldName", fieldName);
            command.Parameters.AddWithValue("$fieldValue", fieldValue);
            command.Parameters.AddWithValue("$userID", userID);

            if (await command.ExecuteNonQueryAsync() != 1)
                return DatabaseError.ERROR_USERID_NOT_EXISTS;
            else
                return DatabaseError.SUCCESS;
        }

        public Task<DatabaseError> UpdatePasswordsDataAsync(int userID, byte[] passwordsData)
        {
            return UpdateBytesfield(userID, "passwordsData", passwordsData);
        }

        public Task<DatabaseError> UpdateDevicesAndPublicKeysInfoAsync(int userID, List<(string, string)> deviceAndPublicKeyList)
        {
            var devicesAndPublicKeys = new DevicesAndPublicKeys();

            foreach (var deviceAndPublicKeyTuple in deviceAndPublicKeyList)
            {
                var deviceAndPublicKey = new DeviceAndPublicKey()
                {
                    DeviceID = deviceAndPublicKeyTuple.Item1,
                    PublicKey = deviceAndPublicKeyTuple.Item2
                };
                devicesAndPublicKeys.DeviceAndPublicKey.Add(deviceAndPublicKey);
            }

            using var ms = new System.IO.MemoryStream();
            devicesAndPublicKeys.WriteTo(ms);

            return UpdateBytesfield(userID, "devicesAndPublicKeysInfo", devicesAndPublicKeys.ToByteArray());
        }

        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<DatabaseError> DeleteAccountAsync(int userID)
        {
            // 这里可以加判断，判断用户ID是否是存在且唯一的

            using var command = connection_.CreateCommand();
            command.CommandText = "DELETE FROM user_data WHERE userID = $userID";
            //command.CommandText = "DELETE FROM user_data WHERE userID = $userID LIMIT 1";
            command.Parameters.AddWithValue("$userID", userID);

            var result = await command.ExecuteNonQueryAsync();

            // 删除失败
            if (result == 0)
                return DatabaseError.ERROR_USERID_NOT_EXISTS;

            // 大于1说明存在重复的ID
            Debug.Assert(result == 1);

            return DatabaseError.SUCCESS;
        }
    }
}
