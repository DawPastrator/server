using System;
using System.Collections.Generic;
using DawPastrator.Server.Services;
using System.Diagnostics;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task TestDatabsae()
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

            var userName = "cjw";
            var masterPassword = "12adbwjkdwahljdaw31==";

            Debug.Assert(await dbHelper.CreateAccountAsync(userName, masterPassword) == DatabaseError.SUCCESS);

            var userID = await dbHelper.GetUserIDAsync(userName);

            var passwordsData = new byte[10];
            Random rnd = new Random();
            rnd.NextBytes(passwordsData);

            var devicesAndPublicKeysInfo = new List<(string, string)>()
            {
                ("device1", "public key1"),
                ("device2", "public key2")
            };

            Debug.Assert(await dbHelper.UpdatePasswordsDataAsync(userID, passwordsData) == DatabaseError.SUCCESS);
            Debug.Assert(await dbHelper.UpdateDevicesAndPublicKeysInfoAsync(userID, devicesAndPublicKeysInfo) == DatabaseError.SUCCESS);

            Debug.Assert(await dbHelper.VerifyMasterPasswordAsync(userName, masterPassword));

            var newPassword = "new password";
            Debug.Assert(await dbHelper.UpdateMasterPasswordAsync(userID, newPassword) == DatabaseError.SUCCESS);


            var info = await dbHelper.GetDevicesAndPublicKeysInfoAsync(userID);
            Debug.Assert(info.SequenceEqual(devicesAndPublicKeysInfo));

            Debug.Assert(await dbHelper.DeleteAccountAsync(userID) == DatabaseError.SUCCESS);
        }
    }
}
