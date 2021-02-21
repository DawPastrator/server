using DawPastrator.Server.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Services
{
    public interface IDataStorageService
    {
        public Task Store(string userId, DataStorageModel model);
        public Task<DataStorageModel?> Get(string userId);
    }

    public class DefaultDataStorageService : IDataStorageService
    {
        public Task<DataStorageModel?> Get(string userId)
        {
            if (userId == "")
            {
                return Task.FromResult(default(DataStorageModel));
            }
            else
            {
                return Task.FromResult(new DataStorageModel
                {
                    Time = DateTime.Now,
                    Bs4Data = "abcdefg"
                });
            }
        }

        public Task Store(string userId, DataStorageModel model)
        {
            return Task.CompletedTask;
        }

        
    }
}
