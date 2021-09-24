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
using System.Collections;

namespace DawPastrator.Server.Services
{


    public class DatabaseException : Exception
    {
        public enum DatabaseError
        {
            ERROR_UNKNOWN_ERROR,
            ERROR_USERID_NOT_EXISTS,
            ERROR_USERNAME_NOT_EXISTS,
            ERROR_USERNAME_HAS_ALREADY_EXISTS,
            ERROR_USERNAME_IS_NULL_OR_WHITESPACE,
            ERROR_MASTERPASSWORD_IS_NULL_OR_WHITESPACE,
            ERROR_FAIL_TO_CREATE_TABLE,
            ERROR_FAIL_TO_INSERT_ROW
        };

        // 默认值：未知错误
        public DatabaseError ErrorCode { get; private set; } = DatabaseError.ERROR_UNKNOWN_ERROR;

        public static readonly Dictionary<DatabaseError, string> ErrorMessage = new()
        {
            { DatabaseError.ERROR_UNKNOWN_ERROR, "未知错误" },
            { DatabaseError.ERROR_USERID_NOT_EXISTS, "用户ID不存在" },
            { DatabaseError.ERROR_USERNAME_NOT_EXISTS, "用户名不存在" },
            { DatabaseError.ERROR_USERNAME_HAS_ALREADY_EXISTS, "用户名已经存在" },
            { DatabaseError.ERROR_USERNAME_IS_NULL_OR_WHITESPACE, "用户名为空" },
            { DatabaseError.ERROR_MASTERPASSWORD_IS_NULL_OR_WHITESPACE, "主密码为空" },
            { DatabaseError.ERROR_FAIL_TO_CREATE_TABLE, "建表失败" },
            { DatabaseError.ERROR_FAIL_TO_INSERT_ROW, "插入表格失败" },
        };

        public DatabaseException()
            :base(DatabaseException.ErrorMessage[ErrorCode]);
        {
        }

        public DatabaseException(string message)
            : base(message)
        { }

        public DatabaseException(string message, Exception inner)
            : base(message, inner)
        { }

        public DatabaseException(DatabaseError errorCode)
            : this(err)
            base(DatabaseException.ErrorMessage[errorCode])
        {throw new Exception(); }
    }

    /// <summary>
    /// 实现数据库的CRUD
    /// 所有错误通过异常抛出，不使用错误码
    /// 因此调用方法时没有捕获异常即为成功
    /// </summary>
    public interface IDatabaseServices : IDisposable
    {
        /// <summary>
        /// 创建账号
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="masterPassword"></param>
        /// <returns>创建用户生成的userId</returns>
        Task<int> CreateAccountAsync(string userName, string masterPassword);

        /// <summary>
        /// 通过用户名获取用户ID
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>userId</returns>
        Task<int> GetUserIdAsync(string userName);

        // @TODO: 返回值修改
        //Task<List<(string, string)>> GetDevicesAndPublicKeysInfoAsync(int userId);

        /// <summary>
        /// 验证密码正确性
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="masterPassword"></param>
        /// <returns>密码正确为true，错误为false</returns>
        Task<bool> VerifyMasterPasswordAsync(string userName, string masterPassword);

        /// <summary>
        /// 更新存储的密码数据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="passwordsData"></param>
        Task UpdatePasswordsDataAsync(int userId, string passwordsData);

        /// <summary>
        /// 更新主密码
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="masterPassword">新的主密码</param>
        /// <param name="passwordsData">
        /// 由于更新主密码的同时，密码数据需要用新的主密码做加密，因此也需要修改存储密码数据
        /// </param>
        /// <returns></returns>
        Task UpdateMasterPasswordAsync(int userId, string masterPassword, string passwordsData);

        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="publicKey"></param>
        /// <returns>添加设备生成的deviceId</returns>
        Task<int> AddDevice(int userId, string publicKey);

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        Task RemoveDevice(int userId, int deviceId);

        /// <summary>
        /// 销毁账户
        /// </summary>
        /// <param name="userId"></param>
        Task DeleteAccountAsync(int userId);

        /// <summary>
        /// 获取存储的密码数据
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>存储的密码数据，是加密后的base64字符串</returns>
        Task<string> GetPasswordsData(int userId);
    }

    public class SqliteDatabaseServices : IDatabaseServices
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

        public bool UserNameHasAlreadyExists(string userName)
        {
            var a = db.userInfos.Single(b => b.UserName == userName);
            return true;
        }

        public async Task<int> CreateAccountAsync(string userName, string masterPassword)
        {

            //throw new NotImplementedException();
            if (string.IsNullOrWhiteSpace(userName))
                return DatabaseError.ERROR_USERNAME_IS_NULL_OR_WHITESPACE;

            if (string.IsNullOrWhiteSpace(masterPassword))
                return DatabaseError.ERROR_MASTERPASSWORD_IS_NULL_OR_WHITESPACE;

            if (UserNameHasAlreadyExists(userName))
                return DatabaseError.ERROR_USERNAME_NOT_EXISTS;

            // user_data
            //command.CommandText =
            //    @"INSERT INTO user_data (userID, userName, masterPassword, passwordsData ,devicesAndPublicKeysInfo)
            //        VALUES(null, $userName, $masterPassword, null, null)";

            //command.Parameters.AddWithValue("$userName", userName);
            //command.Parameters.AddWithValue("$masterPassword", masterPassword.AddSaltAndEncrypt(userName));

            //if (await command.ExecuteNonQueryAsync() != 1)
            //    return DatabaseError.ERROR_FAIL_TO_INSERT_ROW;

            //return DatabaseError.SUCCESS;
        }

        public async Task<int> GetUserIdAsync(string userName)
        {
            //using var command = connection_.CreateCommand();
            //command.CommandText = "SELECT userID FROM user_data WHERE userName = $userName";
            //command.Parameters.AddWithValue("$userName", userName);

            //using var reader = await command.ExecuteReaderAsync();

            //if (reader.Read())
            return 1;
            //else
            //    throw new Exception("用户名不存在");
        }

        private async Task<string> GetUserName(int userID)
        {
            //using var command = connection_.CreateCommand();
            //command.CommandText = "SELECT userName FROM user_data WHERE userID = $userID";
            //command.Parameters.AddWithValue("$userID", userID);

            //using var reader = await command.ExecuteReaderAsync();

            //if (reader.Read())
            //    return reader.GetString(0);
            //else
            //    throw new Exception("用户名不存在");
            //return 1;
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取主密码
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>主密码</returns>
        private async Task<string> GetMasterPassword(int userID)
        {
            //using var command = connection_.CreateCommand();
            //command.CommandText = "SELECT masterPassword FROM user_data WHERE userID = $userID";
            //command.Parameters.AddWithValue("$userID", userID);

            //using var reader = await command.ExecuteReaderAsync();

            //if (reader.Read())
            //    return reader.GetString(0);
            //else
            //    throw new Exception("用户ID不存在");
            throw new NotImplementedException();
        }

        public async Task<bool> VerifyMasterPasswordAsync(string userName, string masterPassword)
        {
            //var userID = await GetUserIDAsync(userName);
            //var masterPasswordFromDatabase = await GetMasterPassword(userID);
            //var processedMasterPassword = masterPassword.AddSaltAndEncrypt(userName);

            //return masterPasswordFromDatabase == processedMasterPassword;
            throw new NotImplementedException();
        }

        

        /// <summary>
        /// 获取密码数据
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>二进制的密码数据</returns>
        public async Task<string> GetPasswordsData(int userID)
        {
            //return GetBlobData(userID, "passwordsData");
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// 获取设备信息与公私钥信息
        ///// </summary>
        ///// <param name="userID">用户ID</param>
        ///// <returns>关于设备信息的二进制数据</returns>
        //public async Task<List<(string, string)>> GetDevicesAndPublicKeysInfoAsync(int userID)
        //{
        //    var bytesData = await GetBlobData(userID, "devicesAndPublicKeysInfo");

        //    var output = new List<(string, string)>();

        //    var devicesAndPublicKeys = DevicesAndPublicKeys.Parser.ParseFrom(bytesData);

        //    foreach (var deviceAndPublicKey in devicesAndPublicKeys.DeviceAndPublicKey)
        //    {
        //        output.Add((deviceAndPublicKey.DeviceID, deviceAndPublicKey.PublicKey));
        //    }

        //    return output;
        //}

        ///// <summary>
        ///// 更新user_data中的字符串字段
        ///// </summary>
        ///// <param name="userID"></param>
        ///// <param name="fieldName">字段名</param>
        ///// <param name="fieldValue">字段值</param>
        //private async Task<DatabaseError> UpdateStringfield(int userID, string fieldName, string fieldValue)
        //{
        //    // 这里可以加上判断，判断用户ID是否存在、是否是唯一的。

        //    //using var command = connection_.CreateCommand();
        //    //command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
        //    ////command.Parameters.AddWithValue("$fieldName", fieldName);
        //    //command.Parameters.AddWithValue("$fieldValue", fieldValue);
        //    //command.Parameters.AddWithValue("$userID", userID);

        //    //if (await command.ExecuteNonQueryAsync() != 1)
        //    //    return DatabaseError.ERROR_USERID_NOT_EXISTS;

        //    //return DatabaseError.SUCCESS;
        //}

        public async Task UpdateMasterPassword(int userID, string masterPassword)
        {
            //var userName = await GetUserName(userID);
            //// 以userID的值作为pbe加密的盐进行加密
            //return await UpdateStringfield(userID, "masterPassword", masterPassword.AddSaltAndEncrypt(userName));

            throw new NotImplementedException();
        }

        //private async Task<DatabaseError> UpdateBytesfield(int userID, string fieldName, byte[] fieldValue)
        //{
        //    using var command = connection_.CreateCommand();
        //    command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
        //    //command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID LIMIT 1";
        //    //command.Parameters.AddWithValue("$fieldName", fieldName);
        //    command.Parameters.AddWithValue("$fieldValue", fieldValue);
        //    command.Parameters.AddWithValue("$userID", userID);

        //    if (await command.ExecuteNonQueryAsync() != 1)
        //        return DatabaseError.ERROR_USERID_NOT_EXISTS;
        //    else
        //        return DatabaseError.SUCCESS;
        //}

        public async Task UpdatePasswordsDataAsync(int userID, string passwordsData)
        {
            throw new NotImplementedException();
        }

        //public Task<DatabaseError> UpdateDevicesAndPublicKeysInfoAsync(int userID, List<(string, string)> deviceAndPublicKeyList)
        //{
        //    var devicesAndPublicKeys = new DevicesAndPublicKeys();

        //    foreach (var deviceAndPublicKeyTuple in deviceAndPublicKeyList)
        //    {
        //        var deviceAndPublicKey = new DeviceAndPublicKey()
        //        {
        //            DeviceID = deviceAndPublicKeyTuple.Item1,
        //            PublicKey = deviceAndPublicKeyTuple.Item2
        //        };
        //        devicesAndPublicKeys.DeviceAndPublicKey.Add(deviceAndPublicKey);
        //    }

        //    using var ms = new System.IO.MemoryStream();
        //    devicesAndPublicKeys.WriteTo(ms);

        //    return UpdateBytesfield(userID, "devicesAndPublicKeysInfo", devicesAndPublicKeys.ToByteArray());
        //}

        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task DeleteAccountAsync(int userID)
        {
            //// 这里可以加判断，判断用户ID是否是存在且唯一的

            //using var command = connection_.CreateCommand();
            //command.CommandText = "DELETE FROM user_data WHERE userID = $userID";
            ////command.CommandText = "DELETE FROM user_data WHERE userID = $userID LIMIT 1";
            //command.Parameters.AddWithValue("$userID", userID);

            //var result = await command.ExecuteNonQueryAsync();

            //// 删除失败
            //if (result == 0)
            //    return DatabaseError.ERROR_USERID_NOT_EXISTS;

            //// 大于1说明存在重复的ID
            //Debug.Assert(result == 1);

            //return DatabaseError.SUCCESS;
            throw new NotImplementedException();
        }

        public async Task UpdateMasterPasswordAsync(int userId, string masterPassword, string passwordsData)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AddDevice(int userId, string publicKey)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveDevice(int userId, int deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
