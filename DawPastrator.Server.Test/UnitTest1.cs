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
            var dbHelper = new SqliteDatabaseServices();

            string userName = "cjw";
            string masterPassword = "12adbwjkdwahljdaw31==";

            dbHelper.UserNameHasAlreadyExists(userName);

            //Debug.Assert(dbHelper.CreateAccountAsync(userName, masterPassword) == DatabaseError.SUCCESS);

            //int userID = dbHelper.GetUserIDAsync(userName);

            //byte[] passwordsData = new byte[10];
            //Random rnd = new Random();
            //rnd.NextBytes(passwordsData);

            //var devicesAndPublicKeysInfo = new List<Tuple<string, string>>();
            //devicesAndPublicKeysInfo.Add(new Tuple<string, string>("device1", "public key1"));
            //devicesAndPublicKeysInfo.Add(new Tuple<string, string>("device2", "public key2"));

            //Debug.Assert(dbHelper.UpdatePasswordsDataAsync(userID, passwordsData) == DatabaseError.SUCCESS);
            //Debug.Assert(dbHelper.UpdateDevicesAndPublicKeysInfo(userID, devicesAndPublicKeysInfo) == DatabaseError.SUCCESS);

            //Debug.Assert(dbHelper.VerifyMasterPasswordAsync(userName, masterPassword));

            //string newPassword = "new password";
            //Debug.Assert(dbHelper.UpdateMasterPassword(userID, newPassword) == DatabaseError.SUCCESS);

            //var info = dbHelper.GetDevicesAndPublicKeysInfoAsync(userID);
            //Debug.Assert(info.SequenceEqual(devicesAndPublicKeysInfo));

            //Debug.Assert(dbHelper.DeleteAccountAsync(userID) == DatabaseError.SUCCESS);
        }
    }
}
