using DawPastrator.Server.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Services
{
    public interface IDataStorageService
    {
        public Task Store(int userId, DataStorageModel model);
        public Task<DataStorageModel?> Get(int userId);
    }

    public class DefaultDataStorageService : IDataStorageService
    {
        public Task<DataStorageModel?> Get(int userId)
        {
            if (userId == 25565)
            {
                return Task.FromResult(default(DataStorageModel));
            }
            else
            {
                return Task.FromResult(new DataStorageModel
                {
                    Bs4Data = "abcdefg"
                });
            }
        }

        public Task Store(int userId, DataStorageModel model)
        {
            return Task.CompletedTask;
        }

        
    }
}
