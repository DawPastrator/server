using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace DawPastrator.Server.Services
{
    /// <summary>
    /// 实现数据库的CRUD
    /// </summary>
    public interface IDatabaseServices
    {
        // TODO 返回状态码而不是bool

        bool CreateTables();
        bool CreateAccount(in string userName, in string masterPassword);

        int GetUserID(in string userName);

        string GetMasterPassword(in int userID);

        byte[] GetPasswordsData(in int userID);

        byte[] GetDevicesAndPublicKeysInfo(in int userID);

        bool UpdateMasterPassword(in int userID, in string masterPassword);

        bool UpdatePasswordsData(in int userID, in byte[] passwordsData);

        bool UpdateDevicesAndPublicKeysInfo(in int userID, in byte[] devicesAndPublicKeysInfo);

        bool deleteAccount(in int userID);
    }

    class ISqliteDatabaseServices : IDatabaseServices
    {
        private readonly SqliteConnection connection_;

        public ISqliteDatabaseServices()
        {
            connection_ = new SqliteConnection("Data Source=data.db");
            connection_.Open();
        }

        ~ISqliteDatabaseServices()
        {
            connection_.Close();
        }

        private bool TableHasBeenCreated(in string tableName)
        {
            Console.WriteLine("Checking if the {0} table has been created.", tableName);

            using SqliteCommand command = connection_.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE name = $tableName";
            command.Parameters.AddWithValue("$tableName", tableName);

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                Console.WriteLine("The {0} table has been created.", tableName);
                return true;
            }
            else
            {
                Console.WriteLine("The {0} table has not been created.", tableName);
                return false;
            }
        }

        public bool CreateTables()
        {
            bool hasAlreadyCreated = true;

            // Deprecated:
            //if (!TableHasBeenCreated("user_info"))
            //{
            //    using SqliteCommand command = connection_.CreateCommand();
            //    command.CommandText =
            //    @"
            //    create table user_info (
            //        userName varchar[40] primary key not null,
            //        userID Integer AUTOINCREMENT
            //    )
            //    ";

            //    command.ExecuteNonQuery();
            //    hasAlreadyCreated = false;
            //}

            if (!TableHasBeenCreated("user_data"))
            {
                using SqliteCommand command = connection_.CreateCommand();

                command.CommandText =
                @"
                CREATE TABLE user_data (
                    userID INTEGER PRIMARY KEY AUTOINCREMENT,
                    userName varchar[40] NOT NULL,
                    masterPassword CHAR[44] NOT NULL,
                    passwordsData BLOB,
                    devicesAndPublicKeysInfo BLOB
                )
                ";
                command.ExecuteNonQuery();

                // 前缀索引
                command.CommandText = "CREATE INDEX userNameIndex ON user_data (substr(userName, 0, 4));";
                command.ExecuteNonQuery();

                hasAlreadyCreated = false;
            }

            return hasAlreadyCreated || TableHasBeenCreated("user_data");
        }

        private bool UserNameHasAlreadyExists(in string userName)
        {
            using var command = connection_.CreateCommand();

            command.CommandText = "SELECT userName FROM user_data WHERE userName = $userName";
            command.Parameters.AddWithValue("$userName", userName);

            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        public bool CreateAccount(in string userName, in string masterPassword)
        {
            using var command = connection_.CreateCommand();

            // ！！在调用该函数之前应该做充分的检查，确保userName不和数据库中已有用户名重复

            if (userName == string.Empty || masterPassword == string.Empty)
                throw new ArgumentException("用户名或密码为空！");

            if (UserNameHasAlreadyExists(userName))
                throw new ArgumentException("用户名已存在！");

            // user_data
            command.CommandText =
                @"INSERT INTO user_data (userID, userName, masterPassword, passwordsData ,devicesAndPublicKeysInfo)
                    VALUES(null, $userName, $masterPassword, null, null)";

            command.Parameters.AddWithValue("$userName", userName);
            command.Parameters.AddWithValue("$masterPassword", masterPassword);
            command.ExecuteNonQuery();

            return true;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>用户ID，或者-1表示失败</returns>
        public int GetUserID(in string userName)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT userID FROM user_data WHERE userName = $userName";
            command.Parameters.AddWithValue("$userName", userName);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取主密码
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>主密码，或者空字符串表示失败</returns>
        public string GetMasterPassword(in int userID)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT masterPassword FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return reader.GetString(0);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取二进制数据
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="requiredfield">要求获得的字段名</param>
        /// <returns>二进制数据</returns>
        private byte[] GetBlobData(in int userID, in string requiredfield)
        {
            using var command = connection_.CreateCommand();
            command.CommandText = "SELECT " + requiredfield + " FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);
            //command.Parameters.AddWithValue("$requiredfield", requiredfield);

            using var reader = command.ExecuteReader();
            bool hasData = reader.Read();

            Debug.Assert(hasData);

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
        public byte[] GetPasswordsData(in int userID)
        {
            return GetBlobData(userID, "passwordsData");
        }

        /// <summary>
        /// 获取设备信息与公私钥信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>被序列化的二进制数据</returns>
        public byte[] GetDevicesAndPublicKeysInfo(in int userID)
        {
            return GetBlobData(userID, "devicesAndPublicKeysInfo");
        }

        /// <summary>
        /// 更新user_data中的字符串字段
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldValue">字段值</param>
        /// <returns>成功返回true否则false</returns>
        private bool UpdateStringfield(in int userID, in string fieldName, in string fieldValue)
        {
            // 这里可以加上判断，判断用户ID是否存在、是否是唯一的。

            using var command = connection_.CreateCommand();
            command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
            //command.Parameters.AddWithValue("$fieldName", fieldName);
            command.Parameters.AddWithValue("$fieldValue", fieldValue);
            command.Parameters.AddWithValue("$userID", userID);
            return command.ExecuteNonQuery() == 1;
        }

        public bool UpdateMasterPassword(in int userID, in string masterPassword)
        {
            return UpdateStringfield(userID, "masterPassword", masterPassword);
        }

        private bool UpdateBytesfield(in int userID, in string fieldName, in byte[] fieldValue)
        {
            // 这里可以加上判断，判断用户ID是否存在、是否是唯一的。

            using var command = connection_.CreateCommand();
            command.CommandText = "UPDATE user_data SET " + fieldName + " = $fieldValue WHERE userID = $userID";
            //command.Parameters.AddWithValue("$fieldName", fieldName);
            command.Parameters.AddWithValue("$fieldValue", fieldValue);
            command.Parameters.AddWithValue("$userID", userID);
            return command.ExecuteNonQuery() == 1;
        }

        public bool UpdatePasswordsData(in int userID, in byte[] passwordsData)
        {
            return UpdateBytesfield(userID, "passwordsData", passwordsData);
        }

        public bool UpdateDevicesAndPublicKeysInfo(in int userID, in byte[] devicesAndPublicKeysInfo)
        {
            return UpdateBytesfield(userID, "devicesAndPublicKeysInfo", devicesAndPublicKeysInfo);
        }

        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool deleteAccount(in int userID)
        {
            // 这里可以加判断，判断用户ID是否是存在且唯一的

            using var command = connection_.CreateCommand();
            command.CommandText = "DELETE FROM user_data WHERE userID = $userID";
            command.Parameters.AddWithValue("$userID", userID);
            return command.ExecuteNonQuery() == 1;
        }
    }
}
