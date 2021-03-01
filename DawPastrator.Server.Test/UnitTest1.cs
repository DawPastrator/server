using System;
using System.Collections.Generic;
using DawPastrator.Server.Services;
using System.Diagnostics;
using Xunit;
using System.Linq;

namespace DawPastrator.Server.Test
{
    public class UnitTest
    {

        private readonly Xunit.Abstractions.ITestOutputHelper _testOutputHelper;

        public UnitTest(Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestDatabsae()
        {
            if (System.IO.File.Exists("data.db"))
            {
                // Use a try block to catch IOExceptions, to
                // handle the case of the file already being
                // opened by another process.
                try
                {
                    System.IO.File.Delete("data.db");
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            var dbHelper = new SqliteDatabaseServices();

            string userName = "cjw";
            string masterPassword = "12adbwjkdwahljdaw31==";

            Debug.Assert(dbHelper.CreateAccount(userName, masterPassword) == DatabaseError.SUCCESS);

            int userID = dbHelper.GetUserID(userName);

            byte[] passwordsData = new byte[10];
            Random rnd = new Random();
            rnd.NextBytes(passwordsData);

            var devicesAndPublicKeysInfo = new List<Tuple<string, string>>();
            devicesAndPublicKeysInfo.Add(new Tuple<string, string>("device1", "public key1"));
            devicesAndPublicKeysInfo.Add(new Tuple<string, string>("device2", "public key2"));

            Debug.Assert(dbHelper.UpdatePasswordsData(userID, passwordsData) == DatabaseError.SUCCESS);
            Debug.Assert(dbHelper.UpdateDevicesAndPublicKeysInfo(userID, devicesAndPublicKeysInfo) == DatabaseError.SUCCESS);

            Debug.Assert(dbHelper.VerifyMasterPassword(userName, masterPassword));

            string newPassword = "new password";
            Debug.Assert(dbHelper.UpdateMasterPassword(userID, newPassword) == DatabaseError.SUCCESS);

            var info = dbHelper.GetDevicesAndPublicKeysInfo(userID);
            Debug.Assert(info.SequenceEqual(devicesAndPublicKeysInfo));

            Debug.Assert(dbHelper.DeleteAccount(userID) == DatabaseError.SUCCESS);
        }
    }
}
