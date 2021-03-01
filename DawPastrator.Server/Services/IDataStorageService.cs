using DawPastrator.Server.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Services
{
    public interface IDataStorageService
    {
        public Task StoreAsync(int userId, DataStorageModel model);
        public Task<DataStorageModel?> GetAsync(int userId);
    }

    public class DefaultDataStorageService : IDataStorageService
    {

        private readonly IDatabaseServices databaseServices;

        public DefaultDataStorageService(IDatabaseServices databaseServices)
        {
            this.databaseServices = databaseServices;
        }

        public Task<DataStorageModel?> GetAsync(int userId)
        {
            //var bytes = databaseServices.GetDevicesAndPublicKeysInfo(userID);
            //var bs4Data = Convert.ToBase64String(bytes);

            return Task.FromResult(new DataStorageModel
            {
                //Bs4Data = bs4Data
            });
        }

        public Task StoreAsync(int userId, DataStorageModel model)
        {
            return Task.CompletedTask;
        }

        
    }
}
