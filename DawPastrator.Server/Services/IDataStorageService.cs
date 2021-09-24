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

        public async Task<DataStorageModel?> GetAsync(int userId)
        {
            var bytes = await databaseServices.GetPasswordsData(userId);
            throw new NotImplementedException();
            //var bs4Data = Convert.ToBase64String(bytes);

            //return new DataStorageModel
            //{
            //    Bs4Data = bs4Data
            //};
        }

        public async Task StoreAsync(int userId, DataStorageModel model)
        {
            var bytes = Convert.FromBase64String(model.Bs4Data);

            //await databaseServices.UpdatePasswordsDataAsync(userId, bytes);
            throw new NotImplementedException();
        }

        
    }
}
